using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace HardWareHelper
{
    public partial class FrmMain : Form
    {
        private HttpListener _httpListener;
        private SerialPort _serialPort;
        private string _comData;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

            if (StartListen())
            {
                WriteLog("Start listening at  http://localhost:8181/");
            }
        }

        /// <summary>
        /// 开始监听Http请求
        /// </summary>
        /// <returns></returns>
        public bool StartListen()
        {
            try
            {
                _httpListener = new HttpListener
                {
                    AuthenticationSchemes = AuthenticationSchemes.Anonymous
                };

                _httpListener.Prefixes.Add("http://localhost:8181/");
                _httpListener.Start();
                _httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                _httpListener.UnsafeConnectionNtlmAuthentication = false;
                _httpListener.BeginGetContext(GetContextCallBack, _httpListener);
                var isListening = _httpListener.IsListening;
                return isListening;
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                return false;
            }
        }

        void GetContextCallBack(IAsyncResult ar)
        {
            try
            {
                _httpListener = ar.AsyncState as HttpListener;
                if (_httpListener != null)
                {
                    _httpListener.BeginGetContext(GetContextCallBack, _httpListener);
                    if (_httpListener != null)
                    {
                        HttpListenerContext httpListenerContext = _httpListener.EndGetContext(ar);
                        //过滤资源类路径
                        if (httpListenerContext.Request.Url.ToString().Contains("."))
                        {
                            return;
                        }
                        httpListenerContext.Response.StatusCode = 200;
                        httpListenerContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
                        using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                        {
                            WriteLog("request：" + httpListenerContext.Request.Url);

                            var responseText = ProcessingHandle(httpListenerContext);
                            writer.Write(responseText);
                            WriteLog("response：" + responseText);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception.Message);
            }

        }

        /// <summary>
        /// 监听处理
        /// </summary>
        /// <returns></returns>
        public string ProcessingHandle(HttpListenerContext httpListenerContext)
        {
            var strMethod = httpListenerContext.Request.Url.Segments[1];
            Type type = typeof(FrmMain);
            var method = type.GetMethod(strMethod);
            if (method == null)
            {
                return "不存在的方法，请检查Url拼写！";
            }
            var result = method.Invoke(this, new object[] { httpListenerContext });
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 获取Mac地址
        /// </summary>
        /// <returns></returns>
        public IList<string> GetMacAddresses(HttpListenerContext httpListenerContext)
        {
            var macAddresses = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up && !string.IsNullOrEmpty(ni.GetPhysicalAddress().ToString()))
                {
                    macAddresses.Add(ni.GetPhysicalAddress().ToString());
                }
            }
            return macAddresses;
        }

        public IList<string> GetPrinters(HttpListenerContext httpListenerContext)
        {
            List<string> printers = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
            return printers;
        }

        #region 扫描枪&读卡器

        /// <summary>
        /// 获取串口名称列表
        /// </summary>
        /// <returns></returns>
        public string[] GetSerialPortNames(HttpListenerContext httpListenerContext)
        {
            string[] serialPortNames = SerialPort.GetPortNames();
            return serialPortNames;
        }

        /// <summary>
        /// 打开串口端口
        /// </summary>
        /// <param name="httpListenerContext"></param>
        /// <returns></returns>
        public bool OpenSerialPort(HttpListenerContext httpListenerContext)
        {
            try
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                }
                var portName = httpListenerContext.Request.QueryString["portName"];
                var baudRate = Convert.ToInt32(httpListenerContext.Request.QueryString["baudRate"]);
                _serialPort =
                    new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                    {
                        Encoding = Encoding.GetEncoding("GB2312"),
                        Handshake = Handshake.RequestToSendXOnXOff
                    };
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.Open();
            }
            catch (Exception e)
            {
               WriteLog(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 串口读取数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);//延迟500ms，防止有些硬件设备未写完数据
            var text = _serialPort.ReadExisting();
            _comData = text.Replace("\u0002", "").Replace("\u0003", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }

        /// <summary>
        /// 获取串口数据
        /// </summary>
        /// <returns></returns>
        public string GetComData(HttpListenerContext httpListenerContext)
        {
            while (string.IsNullOrEmpty(_comData))
            {
                Thread.Sleep(100);
            }
            var returnData = _comData;
            _comData = "";
            return returnData;
        }

        /// <summary>
        ///关闭串口
        /// </summary>
        public void CloseSerialPort(HttpListenerContext httpListenerContext)
        {
            _serialPort.Close();
        }



        #endregion

        #region 国办CPU卡

        public bool WriteCpuCard(HttpListenerContext httpListenerContext)
        {
            var port = Convert.ToInt16(httpListenerContext.Request.QueryString["port"]);
            var baud = Convert.ToInt32(httpListenerContext.Request.QueryString["baudRate"]);
            byte[] data = Encoding.UTF8.GetBytes(httpListenerContext.Request.QueryString["data"]);
            string dataBase64String = Convert.ToBase64String(data);
            axCpuCardOCX1.ClosePort();
            var bOpen = axCpuCardOCX1.OpenPort(port, baud);
            Thread.Sleep(500);
            if (bOpen)
            {
                if (axCpuCardOCX1.IsCardAvailabile)
                {
                    return axCpuCardOCX1.SetFileDataBinBase64(dataBase64String);
                }
            }
            return false;
        }


        public string ReadCpuCard(HttpListenerContext httpListenerContext)
        {
            var port = Convert.ToInt16(httpListenerContext.Request.QueryString["port"]);
            var baud = Convert.ToInt32(httpListenerContext.Request.QueryString["baudRate"]);
            axCpuCardOCX1.ClosePort();
            var bOpen = axCpuCardOCX1.OpenPort(port, baud);
            Thread.Sleep(500);
            if (bOpen)
            {
                if (axCpuCardOCX1.IsCardAvailabile)
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(axCpuCardOCX1.GetFileDataBinBase64()));
                }
            }
            return "";
        }

        #endregion


        private delegate void WriteLogDelegate(string message);
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="message"></param>
        private void WriteLog(string message)
        {
            if (txtLog.InvokeRequired == false)
                txtLog.AppendText(DateTime.Now + "|" + message + "\r\n");
            else
            {
                var writeLogDelegate = new WriteLogDelegate(WriteLog);
                txtLog.Invoke(writeLogDelegate, message);
            }
        }
    }
}
