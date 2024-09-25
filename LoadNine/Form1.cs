using System;
using System.Drawing;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using OpenCvSharp;
using System.IO;

namespace LoadNine
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();
        public int count = 0;
        public Form1()
        {
            InitializeComponent();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;    // 좌상단 X 좌표
            public int Top;     // 좌상단 Y 좌표
            public int Right;   // 우하단 X 좌표
            public int Bottom;  // 우하단 Y 좌표
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hWnd, out Rectangle rect);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]

        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetDpiForWindow(IntPtr hWnd);

        public bool searchIMG(Bitmap screen_img, Bitmap find_img, double sim)
        {
            try
            {
                using (Mat ScreenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screen_img))
                using (Mat FindMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(find_img))
                using (Mat res = ScreenMat.MatchTemplate(FindMat, TemplateMatchModes.CCoeffNormed))
                {
                    double minval, maxval = 0;
                    OpenCvSharp.Point minloc, maxloc;
                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                    if (maxval >= sim)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                label8.Text = "Game Load Error(SI)";
                return false;
            }
        }

        /*        public Bitmap GetTargetScreen(string targetWindowName)
                {
                    try
                    {
                        IntPtr findwindow = FindWindow(null, targetWindowName);
                        if (findwindow != IntPtr.Zero)
                        {
                            Graphics Graphicsdata = Graphics.FromHwnd(findwindow);
                            Rectangle rect;
                            GetWindowRect(findwindow, out rect);
                            float dpi = (float)GetDpiForWindow(findwindow) / 96f;
                            int width = (int)(rect.Width * dpi);
                            int height = (int)(rect.Height * dpi);
                            Bitmap bmp = new Bitmap(width, height);

                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                IntPtr hdc = g.GetHdc();
                                PrintWindow(findwindow, hdc, 0x2);
                                g.ReleaseHdc(hdc);
                            }
                            return bmp;
                        }
                        return new Bitmap(1, 1);
                    }
                    catch (Exception ex)
                    {
                        label8.Text = "Game Load Error(GTS)";
                        MessageBox.Show(ex.ToString());
                        return null;
                    }
                }*/

        public Bitmap GetTargetScreen(string targetWindowName)
        {
            Bitmap bmp = null;
            Graphics g = null;

            try
            {
                IntPtr findwindow = FindWindow(null, targetWindowName);
/*                // 핸들 유효성 확인
                if (findwindow == IntPtr.Zero)
                {
                    label8.Text = "Game window not found.";
                    return null;
                }

                // 게임 창이 보이는지 확인
                if (!IsWindowVisible(findwindow))
                {
                    label8.Text = "Game window is not visible.";
                    return null;
                }*/

                using (Graphics Graphicsdata = Graphics.FromHwnd(findwindow))
                {
                    Rectangle rect;
                    GetWindowRect(findwindow, out rect);
                    float dpi = (float)GetDpiForWindow(findwindow) / 96f;
                    int width = (int)(rect.Width * dpi);
                    int height = (int)(rect.Height * dpi);

                    /*if (width <= 0 || height <= 0)
                    {
                        label8.Text = "Invalid window dimensions.";
                        return null;
                    }*/

                    bmp = new Bitmap(width, height);

                    g = Graphics.FromImage(bmp);
                    IntPtr hdc = g.GetHdc();
                    if (!PrintWindow(findwindow, hdc, 0x2))
                    {
                        throw new Exception("PrintWindow failed.");
                    }
                    g.ReleaseHdc(hdc);
                }

                return bmp;
            }
            catch (Exception ex)
            {
                label8.Text = "Game Load Error(GTS)";
                timer1.Stop();
                label3.Text = "Stop...";
                count = 0;
                button1.Enabled = true; // 버튼 다시 활성화
                MessageBox.Show("게임 인식 에러!!! \n 게임이 종료되었다면 다시 켜주시고 게임 최소화를 풀어주세요.","경고");
                
                return null;
            }
            finally
            {
                if (g != null)
                {
                    g.Dispose();
                }
                if (bmp != null && bmp.Width == 1 && bmp.Height == 1)
                {
                    bmp.Dispose();
                }
            }
        }







        public bool SimpleCheck(string Path, string targetWindowName)
        {
            Bitmap screen = null;
            Bitmap findimg = null;
            try
            {
                screen = GetTargetScreen(targetWindowName);
                if (screen == null)
                {
                    return false;
                }
                findimg = new Bitmap(Path);
                label8.Text = "Not Error";
                return searchIMG(screen, findimg, 0.5);
            }
            catch (Exception)
            {
                label8.Text = "Game Load Error(SC)";
                return false;
            }
            finally
            {
                if(screen != null)
                {
                    screen.Dispose();
                }
                if(findimg != null)
                {
                    findimg.Dispose();
                }
            }
        }

        private async Task<string> SendPostRequestAsync(object data)
        {
            try
            {
                // 데이터 객체를 JSON 문자열로 직렬화 (Newtonsoft.Json 사용)
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // POST 요청 보내기
                HttpResponseMessage response = await client.PostAsync("http://jungsonghun.iptime.org:9999", content);

                // 응답 상태 코드 확인
                if (response.IsSuccessStatusCode)
                {
                    // 응답 본문 읽기
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // 오류 메시지 반환
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                timer1.Stop();
                label3.Text = "Stop...";
                count = 0;
                button1.Enabled = true; // 버튼 다시 활성화
                MessageBox.Show("서버 오류");
                return $"Exception: {ex.Message}";
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                string name = textBox1.Text;
                if (name == "")
                {
                    MessageBox.Show("디스코드에 등록한 이름을 입력해주세요.", "알림 메세지");
                    return;
                }

                button1.Enabled = false; // 버튼 비활성화
                timer1.Start();
                count += 1;
                label5.Text = count.ToString();
                label3.Text = "Running..";               

                if (radioButton1.Checked)
                {
                    if (SimpleCheck("findtoimage\\FHD.PNG", "LORDNINE"))
                    {
                        await SendPostRequestAsync(new { name = name });
                        button2_Click(sender, e);
                    }
                }
                else if (radioButton2.Checked)
                {
                    if (SimpleCheck("findtoimage\\QHD.PNG", "LORDNINE"))
                    {
                        await SendPostRequestAsync(new { name = name });
                        button2_Click(sender, e);
                    }
                }
                else if (radioButton3.Checked)
                {
                    button2_Click(sender, e);
                    MessageBox.Show("개발중인 해상도 입니다.");
                }
                else if (radioButton4.Checked)
                {
                    if (File.Exists("findtoimage\\USER.PNG"))
                    {
                        if (SimpleCheck("findtoimage\\USER.PNG", "LORDNINE"))
                        {
                            await SendPostRequestAsync(new { name = name });
                            button2_Click(sender, e);
                        }
                    }
                    else
                    {
                        button2_Click(sender, e);
                        MessageBox.Show("사용자 지정 이미지를 먼저 넣어주세요.", "LORDNINE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                                   
                }
                else
                {
                    button2_Click(sender, e);
                    MessageBox.Show("해상도를 선택해주세요.");
                }
            }
            catch (Exception)
            {
                button2_Click(sender, e);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                timer1.Stop();
                label3.Text = "Stop...";
                count = 0;
                label8.Text = "Not Error";
                button1.Enabled = true; // 버튼 다시 활성화
            }
            catch (Exception)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                button1_Click(sender, e);   
            }
            catch (Exception)
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap screenImage = null;
            try
            {
                if(screenImage != null)
                {
                    screenImage.Dispose();
                }
                screenImage = GetTargetScreen("LORDNINE");

                if (screenImage != null)
                {
                    if (pictureBox2.Image != null)
                    {
                        pictureBox2.Image.Dispose();
                    }
                    pictureBox2.Image = screenImage;
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    label8.Text = "Not Error";
                }
            }
            catch (Exception)
            {
                label8.Text = "Game Load Error(B3)";
            }
        }



        //private bool CheckProcess(string Name)
        //{
        //    Process process;
        //    try
        //    {
        //        process = Process.GetProcessesByName(Name)[0];
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //    if (process == null)
        //    {

        //        return false;
        //    }
        //    else { return true; }
        //}

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    if (!CheckProcess("LORDNINE.exe"))
        //    {
        //        MessageBox.Show("게임을 먼저 실행시켜주세요.", "LORDNINE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        Environment.Exit(0);
        //    }
        //}
    }
}