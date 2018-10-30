using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;


//개발자 : 양지현
//프로그램 개발 : 키넥트를 이용해서 좌표값 출력

namespace WpfTest
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; //실시간 바인딩을 위함 
        //arrange the variables
        private KinectSensor kinectSensor;  //getDefault를 통해 인스턴스를 받을거임. 
        private Body[] bodies = new Body[6]; //kinect can detect 6 people.


        /// <summary>
        /// frame을 읽기 위한 변수 선언 
        /// </summary>
        private MultiSourceFrameReader msfr = null; //to get multiframes
        private MultiSourceFrame msf = null; //will get msf through msfr
        //body frame
        private BodyFrameReader bodyFrameReader;
        private BodyFrame bodyFrame;
        //color frame 화면 출력 변수 선언
        private ColorFrameReader colorFrameReader = null;
        private ColorFrame colorFrame = null;
        private WriteableBitmap colorBitmap = null;
        private FrameDescription crFrameDescrip = null;
      

        /// <summary>
        /// drawing 스켈레톤 그리기를 위함.
        /// </summary>
        List<Tuple<JointType, JointType>> bones;
        private DrawingImage imageSource;
        private DrawingGroup drawingGroup;
        Pen pointPen = null;
        Pen notSurePointPen = null;
        Pen bonesPen = null;
        Pen notSureBonePen = null;
        Pen savedPen = null;

        /// <summary>
        /// 데이터 베이스 관련 변수 elder 
        /// </summary>
        string connectionStr = "SERVER=127.0.0.1; PORT =3306; DATABASE =elder; UID =root; pwd=****; SslMode=none";
        string sql_checkTable = "SELECT count(*) FROM Information_schema.tables WHERE table_name = @tname AND table_schema = 'elder'";



        /// <summary>
        /// 단순 체크를 위한 변수 
        /// </summary>
        bool bodydatareceived = false;
        bool isFrameReady = false;
        /// <summary>
        /// 데이터 저장을 위한 변수 
        /// </summary>
        private IReadOnlyDictionary<JointType, Joint> joints;
        private Dictionary<string, CameraSpacePoint> JointCameraPoints;
        Dictionary<JointType, Point> joint2DPoints;
        Body me = null;

        /// <summary>
        /// display를 위한 String 변수 
        /// </summary>
        string statusInfo = null;
        string tempPointStr = null;
        string allJointPoints = null;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            pointPen = new Pen(Brushes.Blue, 8);
            bonesPen = new Pen(Brushes.Blue, 4);
            notSurePointPen = new Pen(Brushes.LightBlue, 7);
            notSureBonePen = new Pen(Brushes.LightBlue, 4);
            savedPen = new Pen(Brushes.Red, 6);
            if (KinectSensor.GetDefault() != null)
            {
                kinectSensor = KinectSensor.GetDefault(); //센서의 인스턴스를 받아서 지역변수에 저장. 
                ReadersReady();
                this.makeBone(); ///뼈대 생성

                crFrameDescrip = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
                this.colorBitmap = new WriteableBitmap(crFrameDescrip.Width, crFrameDescrip.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

                kinectSensor.Open(); //센서 사용을 위한 오픈 나중에 window 클로우즈 할 때 자원할당 풀어주는거 잊지말기 close!
            }

            JointCameraPoints = new Dictionary<string, CameraSpacePoint>();
           
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();
            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            this.DataContext = this;

            InitializeComponent();
        }
        private void ReadersReady()
        {
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            colorFrameReader = kinectSensor.ColorFrameSource.OpenReader();
        }
        private void makeBone()
        {
            bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            msfr = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Body);
            if (msfr != null)
            {
                msfr.MultiSourceFrameArrived += Msfr_MultiSourceFrameArrived;
                StatusInfo = "FrameReader";
            }
            else
            {
                StatusInfo = "FrameReader is null\n";
            }

        }
        private void Msfr_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            msf = e.FrameReference.AcquireFrame();

            this.DisplayCr(); //화면에 출력해주는 함수
            GetBodyPoint(); //body 좌표를 추출하는 함수 

            if (SitBtn.IsPressed)
                DrawingBody(savedPen, savedPen);
            else
                DrawingBody(pointPen, bonesPen);
        }
        private void DisplayCr()
        {
            using (colorFrame = msf.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame != null && colorBitmap != null)
                {
                    crFrameDescrip = colorFrame.FrameDescription;
                    //1920 1080 crFrameDescrip
                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((crFrameDescrip.Width == this.colorBitmap.PixelWidth) && (crFrameDescrip.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer,
                                (uint)(crFrameDescrip.Width * crFrameDescrip.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }
                        this.colorBitmap.Unlock();
                    }

                }
            }
        }
        void GetBodyPoint()
        {
            using (bodyFrame = msf.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    bodydatareceived = true;
                }
                else
                {
                    statusInfo = "bodyframe is null\n";
                    bodydatareceived = false;
                    isFrameReady = false;
                }
            }
            if (bodydatareceived)
            {
                getBodyJointsPoints(); //관절마다의 좌표값 받기 
            }
          
        }
        private void getBodyJointsPoints()
        {
            //일단은 한 사람만 인식 
          
            me = (from body in bodies where body.IsTracked select body).FirstOrDefault();

            if (me != null)
            {
                joints = me.Joints; //IReadOnlyDictionary<JointType, Joint> joints;
                                    //private Dictionary<string, CameraSpacePoint> JointCameraPoints; 
                joint2DPoints = new Dictionary<JointType, Point>(); //화면 drawing을 위한 2d 좌표 그릇 
                foreach (JointType jointType in joints.Keys)
                {
                    string jointName = jointType.ToString();
                    CameraSpacePoint position = joints[jointType].Position;
                    // sometimes the depth(Z) of an inferred joint may show as negative
                    // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                    if (position.Z < 0)
                    {
                        position.Z = 0.1f;
                    }


                    //2d mapping
                    ColorSpacePoint colorSpacePoint = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(position);
                    this.joint2DPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);


                    //3d point relative to kinect v2
                    if (JointCameraPoints.ContainsKey(jointName)) //if key exists, change value
                    {
                        JointCameraPoints[jointName] = position;
                    }
                    else //add key
                    {
                        JointCameraPoints.Add(jointName, position);
                    }
                    
                    tempPointStr += jointName + "= " + "x: " + (Math.Round(position.X, 3)) + 
                        " y: " + (Math.Round(position.Y, 3)) + " z: " + (Math.Round(position.Z, 3)) + "\n";

                }
                AllJointPoints = tempPointStr;
                tempPointStr = null;
                isFrameReady = true;
            }
            else
            {
                isFrameReady = false;
            }
        }
        private void DrawingBody(Pen vertexPen, Pen bonePen)
        {
            if(isFrameReady)
            {
                using (DrawingContext dc = drawingGroup.Open())
                {

                    //// 테두리 네모 쳐주는 코드 
                    dc.DrawRectangle(null, new Pen(Brushes.Gray, 3), new Rect(0.0, 0.0, (int)colorBitmap.Width, this.colorBitmap.Height));


                    foreach (JointType jointType in joints.Keys) //vertex 그리기 
                    {
                        if (joints[jointType].TrackingState == TrackingState.Tracked)
                        {
                            dc.DrawEllipse(null, vertexPen, joint2DPoints[jointType], 5, 5);
                        }
                        else if (joints[jointType].TrackingState == TrackingState.Inferred)
                        {
                            dc.DrawEllipse(null, notSurePointPen, joint2DPoints[jointType], 5, 5);
                        }
                        else break;

                    }
                    foreach (var bone in this.bones) //뼈대 line 그리기
                    {
                        if (joints[bone.Item1].TrackingState == TrackingState.Tracked && joints[bone.Item2].TrackingState == TrackingState.Tracked)
                        {
                            dc.DrawLine(bonePen, joint2DPoints[bone.Item1], joint2DPoints[bone.Item2]);
                        }
                        else if (joints[bone.Item1].TrackingState == TrackingState.Inferred && joints[bone.Item2].TrackingState == TrackingState.Inferred)
                        {
                            dc.DrawLine(notSureBonePen, joint2DPoints[bone.Item1], joint2DPoints[bone.Item2]);
                        }
                        else break;

                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, (int)colorBitmap.Width, this.colorBitmap.Height));

                }
            }
           
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.colorFrameReader != null || this.bodyFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;

                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
           
            if (this.kinectSensor != null)
            {
                kinectSensor.Close();
                kinectSensor = null;
            }

        }
        public string StatusInfo
        {
            get
            {
                return this.statusInfo;
            }
            set
            {
                if (this.statusInfo != value)
                {
                    this.statusInfo = value;
                    OnPropertyChanged("StatusInfo");
                }
            }
        }
        public string AllJointPoints
        {
            get
            {
                return this.allJointPoints;
            }
            set
            {
                if (this.allJointPoints != value)
                {
                    this.allJointPoints = value;
                    OnPropertyChanged("AllJointPoints");
                }

            }
        }
        public ImageSource ImageSource
        {
            get { return this.imageSource; }
        }
        public ImageSource ColorSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        string CreateTableQuery(string tablename)
        {
            string sql = "create table " + tablename + " (";
            //create table

            foreach (string jointName in JointCameraPoints.Keys)
            {

                if (jointName.Equals("SpineBase"))
                {
                    sql += jointName + "_x double primary key";
                    sql += ", " + jointName + "_y double not null";
                    sql += ", " + jointName + "_z double not null";
                }
                else
                {
                    sql += ", " + jointName + "_x double not null";
                    sql += ", " + jointName + "_y double not null";
                    sql += ", " + jointName + "_z double not null";
                }
            }
            sql += ")";
            return sql;
        }
        void CreateTable(string tablename)
        {
            using (MySqlConnection mysqlConnection = new MySqlConnection(connectionStr))
            {
                try
                {
                    mysqlConnection.Open();
                    MySqlCommand cmd = new MySqlCommand(CreateTableQuery(tablename), mysqlConnection);
                    cmd.ExecuteNonQuery();
                    StatusInfo = tablename.Remove(tablename.Length-2)+ " 자세에 관한 테이블 생성 완료";

                }
                catch (MySqlException sqlEx)
                {
                    MessageBox.Show(sqlEx.StackTrace);
                }
                finally
                {
                    mysqlConnection.Close();
                }
            }
        }
        string InsertQuery(string tablename)
        {
            string sql = "insert into " + tablename + " values (";
            //create table

            foreach (CameraSpacePoint point3d in JointCameraPoints.Values)
            {
                sql += point3d.X +", "+point3d.Y +", " + point3d.Z+ ", ";
            }
            sql = sql.Remove(sql.Length - 2);
            sql += ")";
            return sql;
        }
        void InsertValues(string tablename)
        {
            //table exists insert data
            using (MySqlConnection mysqlConnection = new MySqlConnection(connectionStr))
            {
                try
                {
                    mysqlConnection.Open();
                    MySqlCommand cmd = new MySqlCommand(InsertQuery(tablename), mysqlConnection);
                    cmd.ExecuteNonQuery();
                    StatusInfo = tablename+ " 데이터 저장 성공";

                }
                catch (MySqlException sqlEx)
                {
                    MessageBox.Show(sqlEx.StackTrace);
                }
                finally
                {
                    mysqlConnection.Close();
                }
            }
        }
        bool IsTableExist(string tablename)
        {
            bool Isexist = false;
            using (MySqlConnection mySqlConnection = new MySqlConnection(connectionStr))
            {
                try
                {
                    mySqlConnection.Open();
                    MySqlCommand cmd = new MySqlCommand(sql_checkTable, mySqlConnection);
                    cmd.Parameters.AddWithValue("@tname", tablename); //앉은 자세 training set 에 관한 테이블이 디비에 존재하는 지 확인

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            if (reader.GetInt32(0) == 0)
                                Isexist = false;
                            else
                                Isexist = true;
                        }
                    }
                }
                catch(MySqlException ex)
                {
                    MessageBox.Show(ex.StackTrace);
                }
                finally
                {
                    mySqlConnection.Close();
                   
                }
                return Isexist;
            }
        }
        private void SitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (JointCameraPoints.Count == 0)
            {
                MessageBox.Show("사람을 인식한 후에 저장을 눌러주세요");
                return;
            }
            if (IsTableExist("sitdb"))
            {
                InsertValues("sitdb");
            }
            else
            {
                CreateTable("sitdb");
            }
        }

        private void StandBtn_Click(object sender, RoutedEventArgs e)
        {
            if (JointCameraPoints.Count == 0)
            {
                MessageBox.Show("사람을 인식한 후에 저장을 눌러주세요");
                return;
            }
            if (IsTableExist("standdb"))
            {
                InsertValues("standdb");
            }
            else
            {
                CreateTable("standdb");
            }
        }
    }
}
