using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Timers;
using Timer = System.Timers.Timer;

public class Frame : Form
{
    public ActionPanel actPanel;

    public const int actPanelWidth = 900, actPanelHeight = 600;
    private const int scorePanelWidth = 900, scorePanelHeight = 100;
    private const int frWidth = actPanelWidth, frHeight = actPanelHeight + scorePanelHeight;




    public Frame()
    {
        Icon = Icon.ExtractAssociatedIcon(Environment.CurrentDirectory + "\\Images\\PongIcon.ico");
        StartPosition = FormStartPosition.CenterScreen;
        //Location = new Point(200, 100);
        ClientSize = new Size(frWidth, frHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        KeyPreview = true;

        actPanel = new();
        actPanel.Size = new Size(actPanelWidth, actPanelHeight);
        Controls.Add(actPanel);





    }
    
    protected override void OnKeyDown(KeyEventArgs e) => MarkSelectedKeys(e, true);
    protected override void OnKeyUp(KeyEventArgs e) => MarkSelectedKeys(e, false);

    public bool w, s, up, down;
    private void MarkSelectedKeys(KeyEventArgs e, bool b)
    {
        if(e.KeyCode == Keys.W)
            w = b;
        if(e.KeyCode == Keys.S)
            s = b;
        if(e.KeyCode == Keys.Up)
            up = b;
        if(e.KeyCode == Keys.Down)
            down = b;
    }

    public class ActionPanel : Panel
    {
        public ActionPanel()
        {

            Size = new Size(actPanelWidth, actPanelHeight);
            BackColor = Color.SkyBlue;
            DoubleBuffered = true;
        }

        public Size ballSize { get; private set; } = new Size(15, 15);
        public PointF ballPos;
        public Size blueSize { get; private set; } = new(15, actPanelHeight / 5);
        public Point bluePos;
        public Size redSize { get; private set; } = new (15, actPanelHeight / 5); 
        public Point redPos;
        protected override void OnPaint (PaintEventArgs e)
        {
            base.OnPaint(e);
            SolidBrush brush;
        
            e.Graphics.FillEllipse(brush = new (Color.Black), ballPos.X, ballPos.Y, ballSize.Width, ballSize.Height);
            e.Graphics.FillRectangle(brush = new(Color.Blue), bluePos.X, bluePos.Y, blueSize.Width, blueSize.Height);
            e.Graphics.FillRectangle(brush = new(Color.Red), redPos.X, redPos.Y, redSize.Width, redSize.Height);
        }
    }
    
}

class BackEnd
{

    private Frame frame = new();

    private const float distPerFrame = 10; //the constant velocity of the ball
    private PointF ballMoveDist; //velocity in each axis (calculated automatically)
    private int ballXAxis = 1;
    private Random random = new ();

    private const int frameTime = 5;
    private Timer timer = new(frameTime), delayTimer = new (1000);

    private int newRoundXAxis = 1;
    private bool ballOutBlue = false, ballOutRed = false;
    

    public void Run()
    {
        timer.Elapsed += TimerListener;
        delayTimer.Elapsed += DelayTimerListener;
        
        SetDefaultParameters();
        delayTimer.Start();
        Application.Run(frame);
    }

    private void CreateNewRound()
    {
        timer.Stop();

        newRoundXAxis *= -1;
        SetDefaultParameters();
        frame.actPanel.Refresh();

        Thread.Sleep(1000);
        
        timer.Start();
    }

    private void SetDefaultParameters()
    {
        ballXAxis = newRoundXAxis;
        ballMoveDist = RandomCourseAssign(1);
        frame.actPanel.ballPos = new((Frame.actPanelWidth - frame.actPanel.ballSize.Width) / 2, (Frame.actPanelHeight - frame.actPanel.ballSize.Height) / 2);
        
        frame.actPanel.bluePos = new(0, (Frame.actPanelHeight - frame.actPanel.blueSize.Height) / 2);
        frame.actPanel.redPos = new(Frame.actPanelWidth - frame.actPanel.redSize.Width, (Frame.actPanelHeight - frame.actPanel.redSize.Height) / 2);
    }

    private PointF RandomCourseAssign(float slowDown)
    {
        ballMoveDist.Y = random.Next( (int) -(distPerFrame / 2) , (int) (distPerFrame / 2 ) ) * slowDown;
        return new PointF(ballXAxis * ( (float) Math.Sqrt(Math.Pow(distPerFrame * slowDown , 2) - Math.Pow(ballMoveDist.Y, 2)) ), ballMoveDist.Y);
    }
    private PointF LogicalCourseAssign(int tailPos, float directionChangeHardness)
    {
        ballXAxis *= -1;
        
        int halfTailSize = (frame.actPanel.blueSize.Height + frame.actPanel.ballSize.Height) / 2;
        float ballPosInTail = frame.actPanel.ballPos.Y + frame.actPanel.ballSize.Height - tailPos - halfTailSize;
        ballPosInTail /= halfTailSize;
        
        ballMoveDist.Y = ballPosInTail * distPerFrame * directionChangeHardness;
        return new PointF(ballXAxis * ( (float) Math.Sqrt(Math.Pow(distPerFrame, 2) - Math.Pow(ballMoveDist.Y, 2)) ), ballMoveDist.Y);
    }

    private void MoveTails()
    {
        if (frame.w)
        {
            if (frame.actPanel.bluePos.Y - Frame.actPanelHeight / 50 > 0) frame.actPanel.bluePos.Y -= Frame.actPanelHeight / 50;
            else frame.actPanel.bluePos.Y = 0;
        }
        if (frame.s)
        {
            if (frame.actPanel.bluePos.Y + frame.actPanel.blueSize.Height + Frame.actPanelHeight / 50 < Frame.actPanelHeight) frame.actPanel.bluePos.Y += Frame.actPanelHeight / 50;
            else frame.actPanel.bluePos.Y = Frame.actPanelHeight - frame.actPanel.blueSize.Height;
        }
        if (frame.up)
        {
            if (frame.actPanel.redPos.Y - Frame.actPanelHeight / 50 > 0) frame.actPanel.redPos.Y -= Frame.actPanelHeight / 50;
            else frame.actPanel.redPos.Y = 0;
        }
        if (frame.down)
        {
            if (frame.actPanel.redPos.Y + frame.actPanel.redSize.Height + Frame.actPanelHeight / 50 < Frame.actPanelHeight) frame.actPanel.redPos.Y += Frame.actPanelHeight / 50;
            else frame.actPanel.redPos.Y = Frame.actPanelHeight - frame.actPanel.redSize.Height;
        }
    }

    private void TimerListener(object sender, ElapsedEventArgs e)
    {
        MoveTails();
        
        //checking if the ball collides
        if (ballOutBlue)
        {
            if (frame.actPanel.ballPos.X + frame.actPanel.ballSize.Width < 0)
            {
                ballOutBlue = false;
                CreateNewRound();
            }
            else if (frame.actPanel.ballPos.X > Frame.actPanelWidth / 2) ballOutBlue = false; //to make sure a certain bug doesn't happen 
        }
        else if (ballOutRed)
        {
            if ( frame.actPanel.ballPos.X > Frame.actPanelWidth )
            {
                ballOutRed = false;
                CreateNewRound();
            }
            else if (frame.actPanel.ballPos.X < Frame.actPanelWidth / 2) ballOutRed = false; //to make sure a certain bug doesn't happen

        }
        
        else if ( frame.actPanel.ballPos.X <= frame.actPanel.blueSize.Width )
        {
            if (frame.actPanel.ballPos.Y + frame.actPanel.ballSize.Height > frame.actPanel.bluePos.Y && frame.actPanel.ballPos.Y < frame.actPanel.bluePos.Y + frame.actPanel.blueSize.Height)
            {
                ballMoveDist = LogicalCourseAssign(frame.actPanel.bluePos.Y, 0.8f);
            }
            else ballOutBlue = true;
        }
        else if ( frame.actPanel.ballPos.X + frame.actPanel.ballSize.Width >= frame.actPanel.redPos.X )
        {
            if (frame.actPanel.ballPos.Y + frame.actPanel.ballSize.Height > frame.actPanel.redPos.Y && frame.actPanel.ballPos.Y < frame.actPanel.redPos.Y + frame.actPanel.redSize.Height)
            {
                ballMoveDist = LogicalCourseAssign(frame.actPanel.redPos.Y, 0.8f);
            }
            else ballOutRed = true;
        }
        
        if (frame.actPanel.ballPos.Y <= 0 || frame.actPanel.ballPos.Y + frame.actPanel.ballSize.Height >= Frame.actPanelHeight  )
            ballMoveDist.Y = -ballMoveDist.Y;
        
        //moving the ball
        frame.actPanel.ballPos = new(frame.actPanel.ballPos.X + ballMoveDist.X, frame.actPanel.ballPos.Y + ballMoveDist.Y);
        
        frame.actPanel.Refresh();
    }

    private void DelayTimerListener(object sender, ElapsedEventArgs e)
    {
        delayTimer.Stop();
        timer.Start();
    }

}

static class Program
{
    static void Main()
    {
        BackEnd engine = new();
        engine.Run();
    }
}