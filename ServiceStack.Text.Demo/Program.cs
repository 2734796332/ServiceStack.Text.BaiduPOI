using BaiduBusFiddler;
using Fiddler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.Text.Demo
{
    class Program
    {
        static Proxy oSecureEndpoint = null;
        static string sSecureEndpointHostname = "localhost";
        static int iSecureEndpointPort = 8888;
        static Thread writeThread = new Thread(WriteToDB);
        static Queue<Fiddler.Session> QueueSessions = new Queue<Session>();
        private static void WriteToDB()
        {

            while (true)
            {
                try
                {
                    if (QueueSessions.Count > 0)
                    {
                        Session oSession = QueueSessions.Dequeue();
                        WriteToFiles(oSession);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception e)
                {
                    //File.AppendAllText("d:\\BusPOI_log.txt", e.Message.ToString());
                    continue;
                }
            }
        }
        private static void WriteToFiles(Session dataReceived)
        {
            //oS.utilDecodeResponse();       //针对js可解析         
            if (dataReceived.oResponse.MIMEType == "text/javascript")
            {
                Session iSeesion = new Session(new SessionData(dataReceived));
                while ((iSeesion.state <= SessionStates.ReadingResponse))
                {
                    continue;
                }
                if (dataReceived.fullUrl.Contains("newmap"))
                {
                    iSeesion.utilDecodeResponse();
                    string str = iSeesion.GetResponseBodyAsString();
                    str = Helper.UnicodeToGb(str);
                    str = str.Trim();
                    try
                    {
                        if (str.Contains("place_info"))
                        {
                            //for (int i = 0; i < str.Length; i++)
                            //{
                            //    if (str[i] >= 0x4e00 && str[i] <= 0x9fbb)
                            //    {
                            //        if (str[i - 1] == '\\')
                            //        {
                            //            str = str.Remove(i - 1, 1);
                            //        }
                            //    }
                            //}                 

                            #region 
                            JsonObject jobj = JsonObject.Parse(str);
                            jobj.TryGetValue("content", out str);
                            JsonArrayObjects content = JsonArrayObjects.Parse(str);
                            using (BaiduPOIEntities db = new BaiduPOIEntities())
                            {
                                foreach (var item in content)
                                {
                                    //var model = new baidupoi();
                                    string uid = item.Where(a => a.Key == "uid").FirstOrDefault().Value;
                                    var model = db.baidupoi.Where(a => a.uid == uid).FirstOrDefault();
                                    if (model == null)
                                    {
                                        model = new baidupoi();
                                        model.address = item.Where(a => a.Key == "addr").FirstOrDefault().Value;
                                        model.keyword = "";
                                        model.name = item.Where(a => a.Key == "name").FirstOrDefault().Value;
                                        //model.type = item.Where(a => a.Key == "std_tag").FirstOrDefault().Value!=null?item.Where(a => a.Key == "std_tag").FirstOrDefault().Value:" ";
                                        string cla = item.Where(a => a.Key == "cla").FirstOrDefault().Value;
                                        MatchCollection mc = Regex.Matches(cla, @"[\u4e00-\u9fa5]+");
                                        foreach (Match m in mc)
                                        {
                                            if (m.Success)
                                            {
                                                model.type += m.Value + ";";
                                            }
                                        }
                                        model.uid = item.Where(a => a.Key == "uid").FirstOrDefault().Value;
                                        string x = item.Where(a => a.Key == "x").FirstOrDefault().Value;
                                        model.x = Convert.ToDouble(x.Substring(0, x.Length - 2) + "." + x.Substring(x.Length - 2));
                                        string y = item.Where(a => a.Key == "y").FirstOrDefault().Value;
                                        model.y = Convert.ToDouble(y.Substring(0, y.Length - 2) + "." + y.Substring(y.Length - 2));
                                        db.baidupoi.Add(model);
                                    }
                                }
                                db.SaveChanges();
                            }                   
                            #endregion
                            #region JSON.NET程序
                            /*
                            
                            Newtonsoft.Json.Linq.JObject obj = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(str);
                            JArray content = obj.Value<JArray>("content");
                            using (BaiduPOIEntities db = new BaiduPOIEntities())
                            {
                                foreach (var item in content)
                                {
                                    //var model = new baidupoi();
                                    string uid = item.Value<string>("uid");
                                    var model = db.baidupoi.Where(a => a.uid == uid).FirstOrDefault();
                                    if (model == null)
                                    {
                                        model = new baidupoi();
                                        model.address = item.Value<string>("addr");
                                        model.keyword = "";
                                        model.name = item.Value<string>("name");
                                        model.type = item.Value<string>("std_tag");
                                        model.uid = item.Value<string>("uid");
                                        string x = item.Value<string>("x");
                                        model.x = Convert.ToDouble(x.Substring(0, x.Length - 2) + "." + x.Substring(x.Length - 2));
                                        string y = item.Value<string>("y");
                                        model.y = Convert.ToDouble(y.Substring(0, y.Length - 2) + "." + y.Substring(y.Length - 2));
                                        db.baidupoi.Add(model);
                                    }
                                }
                                db.SaveChanges();
                            } 
                             */
                            #endregion
                        }
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex) 
                    {
                        Console.WriteLine("========");
                        Console.WriteLine("存储错误");
                        Console.WriteLine("========");
                    }

                    catch (Exception e)
                    {
                        
                        Console.WriteLine("========");
                        Console.WriteLine("请处理有问题的JSON");
                        //Console.WriteLine("请处理有问题的JSON：" + e.LinePosition);
                        //Console.WriteLine("请处理有问题的JSON：" + e.Message.ToString());
                        //Console.WriteLine("JSON路径：" + e.Path);
                        Console.WriteLine("========");
                    }

                }



            }
        }


        //public static object GetPropertyValue(object info, string field)
        //{
        //    if (info == null) return null;
        //    Type t = info.GetType();
        //    IEnumerable<System.Reflection.PropertyInfo> property = from pi in t.GetProperties() where pi.Name.ToLower() == field.ToLower() select pi;
        //    return property.First().GetValue(info, null);
        //} 
        /// <summary>
        /// 获取一个类指定的属性值
        /// </summary>
        /// <param name="info">object对象</param>
        /// <param name="field">属性名称</param>
        /// <returns></returns>
        public static object GetPropertyValue(object info, string field)
        {
            if (info == null) return null;
            Type t = info.GetType();
            IEnumerable<System.Reflection.PropertyInfo> property = from pi in t.GetProperties() where pi.Name.ToLower() == field.ToLower() select pi;
            return property.First().GetValue(info, null);
        }


        static void Main(string[] args)
        {
            List<Fiddler.Session> oAllSessions = new List<Fiddler.Session>();

            Fiddler.FiddlerApplication.SetAppDisplayName("FiddlerKiwi");

            #region AttachEventListeners

            Fiddler.FiddlerApplication.OnNotification += delegate(object sender, NotificationEventArgs oNEA)
            {
                Console.WriteLine("**通知: " + oNEA.NotifyString);
            };
            //Fiddler.FiddlerApplication.Log.OnLogString += delegate(object sender, LogEventArgs oLEA)
            //{
            //    Console.WriteLine("**日志: " + oLEA.LogString);
            //};


            Fiddler.FiddlerApplication.BeforeRequest += delegate(Fiddler.Session oS)
            {
                oS.bBufferResponse = false;
                Monitor.Enter(oAllSessions);//添加session时必须加排他锁
                oAllSessions.Add(oS);
                Monitor.Exit(oAllSessions);
                oS["X-AutoAuth"] = "(default)";
                oS.RequestHeaders["Accept-Encoding"] = "gzip, deflate";
                if ((oS.oRequest.pipeClient.LocalPort == iSecureEndpointPort) && (oS.hostname == sSecureEndpointHostname))
                {
                    oS.utilCreateResponseAndBypassServer();
                    oS.oResponse.headers.SetStatus(200, "Ok");
                    oS.oResponse["Content-Type"] = "text/html; charset=UTF-8";
                    oS.oResponse["Cache-Control"] = "private, max-age=0";
                    oS.utilSetResponseBody("<html><body>Request for httpS://" + sSecureEndpointHostname + ":" + iSecureEndpointPort.ToString() + " received. Your request was:<br /><plaintext>" + oS.oRequest.headers.ToString());
                }
            };
            //Kiwi：raw原生的，获得原生数据参数的事件。decompressed（解压缩）chunk（块），gracefully（优雅的地），invalid（无效的），EXACTLY（完全正确）,compatible（兼容的）,Decryption（解码）, E.g.例如，masquerading（伪装）
            Fiddler.FiddlerApplication.AfterSessionComplete += delegate(Fiddler.Session oS)
            {
                if (oS != null)
                {
                    QueueSessions.Enqueue(oS);
                }

            };
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            #endregion AttachEventListeners

            Fiddler.CONFIG.bHookAllConnections = true;
            Fiddler.CONFIG.IgnoreServerCertErrors = true;
            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);

            FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;
            CreateAndTrustRoot();
            int iPort = 8877;//设置为0，程序自动选择可用端口
            writeThread.Start();
            Fiddler.FiddlerApplication.Startup(iPort, oFCSF);
            #region 日志系统
            FiddlerApplication.Log.LogFormat("Created endpoint listening on port {0}", iPort);
            FiddlerApplication.Log.LogFormat("Starting with settings: [{0}]", oFCSF);
            FiddlerApplication.Log.LogFormat("Gateway: {0}", CONFIG.UpstreamGateway.ToString());
            #endregion

            Console.WriteLine("Hit CTRL+C to end session.");

            // We'll also create a HTTPS listener, useful for when FiddlerCore is masquerading（伪装） as a HTTPS server
            // instead of acting as a normal CERN-style proxy server.
            //oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(iSecureEndpointPort, true, sSecureEndpointHostname);
            //if (null != oSecureEndpoint)
            //{
            //    FiddlerApplication.Log.LogFormat("Created secure endpoint listening on port {0}, using a HTTPS certificate for '{1}'", iSecureEndpointPort, sSecureEndpointHostname);
            //}

            bool bDone = false;
            do//使用的是do while
            {
                Console.WriteLine("\nEnter a command [C=Clear; L=List; G=Collect Garbage; R=read SAZ;\n\tS=Toggle Forgetful Streaming; T=Trust Root Certificate; Q=Quit]:");
                Console.Write(">");
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                switch (Char.ToLower(cki.KeyChar))
                {
                    case 'c':
                        Monitor.Enter(oAllSessions);
                        oAllSessions.Clear();
                        Monitor.Exit(oAllSessions);
                        WriteCommandResponse("Clear...");
                        FiddlerApplication.Log.LogString("Cleared session list.");
                        break;

                    case 'd':
                        FiddlerApplication.Log.LogString("FiddlerApplication::Shutdown.");
                        FiddlerApplication.Shutdown();
                        break;

                    case 'l':
                        WriteSessionList(oAllSessions);//【Kiwi】
                        break;

                    case 'g':
                        Console.WriteLine("Working Set:\t" + Environment.WorkingSet.ToString("n0"));
                        Console.WriteLine("Begin GC...");
                        GC.Collect();
                        Console.WriteLine("GC Done.\nWorking Set:\t" + Environment.WorkingSet.ToString("n0"));
                        break;

                    case 'q':
                        bDone = true;
                        DoQuit();
                        break;

                    case 'r':
#if SAZ_SUPPORT
                        ReadSessions(oAllSessions);
#else
                        WriteCommandResponse("This demo was compiled without SAZ_SUPPORT defined");
#endif
                        break;

                    case 't':
                        try
                        {
                            WriteCommandResponse("Result: " + Fiddler.CertMaker.trustRootCert().ToString());
                        }
                        catch (Exception eX)
                        {
                            WriteCommandResponse("Failed: " + eX.ToString());
                        }
                        break;

                    // Forgetful streaming
                    case 's':
                        bool bForgetful = !FiddlerApplication.Prefs.GetBoolPref("fiddler.network.streaming.ForgetStreamedData", false);
                        FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.ForgetStreamedData", bForgetful);
                        Console.WriteLine(bForgetful ? "FiddlerCore will immediately dump streaming response data." : "FiddlerCore will keep a copy of streamed response data.");
                        break;

                }
            } while (!bDone);
        }
        private static bool CreateAndTrustRoot()
        {
            if (!Fiddler.CertMaker.rootCertExists())
            {
                var bCreatedRootCertificate = Fiddler.CertMaker.createRootCert();
                if (!bCreatedRootCertificate)
                {
                    return false;
                }
            }
            if (!Fiddler.CertMaker.rootCertIsTrusted())
            {
                var bTrustedRootCertificate = Fiddler.CertMaker.trustRootCert();
                if (!bTrustedRootCertificate)
                {
                    return false;
                }
            }
            return true;
        }
        private static void WriteSessionList(List<Fiddler.Session> oAllSessions)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Session list contains...");
            //放到一个try块中
            try
            {
                Monitor.Enter(oAllSessions);//Monitor Kiwi-?
                foreach (Session oS in oAllSessions)
                {
                    //MIME Type，资源的媒体类型
                    Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));
                }
            }
            finally
            {
                Monitor.Exit(oAllSessions);
            }
            Console.WriteLine();
            Console.ForegroundColor = oldColor;
        }
        /// <summary>
        /// 超过长度时的显示方式
        /// </summary>
        /// <param name="s"></param>
        /// <param name="iLen"></param>
        /// <returns></returns>
        private static string Ellipsize(string s, int iLen)
        {
            if (s.Length <= iLen) return s;
            return s.Substring(0, iLen - 3) + "...";
        }
        public static void WriteCommandResponse(string s)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ForegroundColor = oldColor;
        }

        #region 退出
        /// <summary>
        /// 退出程序
        /// </summary>
        public static void DoQuit()
        {
            WriteCommandResponse("Shutting down...");
            if (null != oSecureEndpoint) oSecureEndpoint.Dispose();
            Fiddler.FiddlerApplication.Shutdown();
            Thread.Sleep(500);
        }
        /// <summary>
        /// When the user hits CTRL+C, this event fires.  We use this to shut down and unregister our FiddlerCore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DoQuit();
        }
        #endregion
    }
}
