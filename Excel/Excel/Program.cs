using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Excel
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isheader = true;

            //Fetching from URL
            //HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://apps.waterconnect.sa.gov.au/file.csv");
            //HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            //StreamReader reader = new StreamReader(resp.GetResponseStream());
            
            //Fetching from local system(Hard coded)
            //StreamReader reader = new StreamReader(File.OpenRead(@"C:/Users/sjman/Downloads/bugrow.csv"));
            //StreamReader reader = new StreamReader(File.OpenRead(@"E:/Excel/file.csv"));

            //Creating Export,Import,Log folder
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName + "\\";
            string Export = "Export", Import = "Import", Log = "Log";
            string url = "https://apps.waterconnect.sa.gov.au/file.csv";
            string[] urlsegment = url.Split('/');
            string Importfilename = urlsegment[urlsegment.Length - 1];
            string fileext = Importfilename.Split('.')[1];
            string Importfile = Path.Combine(projectDirectory, Import, Importfilename);
            if (!Directory.Exists(projectDirectory + Export))
            {
                Directory.CreateDirectory(projectDirectory + Export);
            }
            if (!Directory.Exists(projectDirectory + Import))
            {
                Directory.CreateDirectory(projectDirectory + Import);
            }
            if (!Directory.Exists(projectDirectory + Log))
            {
                Directory.CreateDirectory(projectDirectory + Log);
            }

            //Use of webclient to download url file to import folder
            using (WebClient requ = new WebClient())
            {
                requ.DownloadFile(url,Importfile);
            }
            string Importpath = Path.Combine(projectDirectory + Import, Importfilename);
            StreamReader reader = new StreamReader(File.OpenRead(Importpath));

            //Destination file in local system
            string Exportfilename = Importfilename.Split('.')[0] + "_" + DateTime.Now.ToString("MMddyyyy") + "_" + DateTime.Now.ToString("hhmmss");
            string Exportpath = Path.Combine(projectDirectory + Export, Exportfilename + "." + fileext);

            //Log file in local system
            string Errorlog = "log.txt";
            string Errorpath = Path.Combine(projectDirectory + Log, Errorlog);

            List<string> headers = new List<string>();
            List<string> lines = new List<string>();
            List<string> eachvalues = new List<string>();

            int indexofUnitNo = 0; int indexofswl = 0; int indexofrswl = 0;

            decimal swl = 0; decimal rswl = 0;

            int Lineno = 0;

            try
            {
                while (!reader.EndOfStream)
                {
                    eachvalues.Clear();
                    string line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line) && !string.IsNullOrEmpty(line) && !line.StartsWith("\""))
                    {
                        string[] cols = line.Split(',');
                        Array.Resize(ref cols, cols.Length + 1);

                        if (isheader)
                        {
                            isheader = false;
                            cols[cols.Length - 1] = "calc";
                            headers = cols.ToList();
                            indexofUnitNo = Array.IndexOf(cols, "Unit_No");
                            indexofswl = Array.IndexOf(cols, "swl");
                            indexofrswl = Array.IndexOf(cols, "rswl");

                            int i = 0;
                            for (i = 0; i < cols.Length; i++)
                            {
                                if (i != indexofUnitNo)
                                    eachvalues.Add(cols[i]);
                            }
                            var newLine = string.Join(",", eachvalues);
                            lines.Add(newLine);
                            Lineno = Lineno + 1;
                        }
                        else
                        {
                            //cols[cols.Length - 1] = " ";
                            swl = Convert.ToDecimal(String.IsNullOrEmpty(cols[indexofswl]) ? (decimal)0 : Convert.ToDecimal(cols[indexofswl]));
                            rswl = Convert.ToDecimal(String.IsNullOrEmpty(cols[indexofrswl]) ? (decimal)0 : Convert.ToDecimal(cols[indexofrswl]));
                            cols[cols.Length - 1] = Convert.ToString(swl + rswl);
                            int i = 0;
                            for (i = 0; i < cols.Length; i++)
                            {
                                if (i != indexofUnitNo)
                                    eachvalues.Add(cols[i]);
                            }
                            var newLine = string.Join(",", eachvalues);
                            lines.Add(newLine);
                            Lineno = Lineno + 1;

                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!File.Exists(Errorpath))
                {
                    File.Create(Errorpath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(Errorpath))
                {
                    sw.WriteLine("=============Error Logging ===========");
                    sw.WriteLine("===========Start============= " + DateTime.Now);
                    sw.WriteLine("Excel Error Line: " + Lineno);
                    sw.WriteLine("Error Message: " + e.Message);
                    sw.WriteLine("Stack Trace: " + e.StackTrace);
                    sw.WriteLine("===========End============= " + DateTime.Now);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
            }

            try
            {
                using (StreamWriter writer = new StreamWriter((new FileStream(Exportpath, FileMode.Create, FileAccess.Write))))
                {
                    foreach (var line in lines)
                    {
                        if (line.Contains("\""))
                        {
                            string trimline = line.Replace('\"', ' ');  
                            writer.WriteLine(trimline);
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                using (StreamWriter sw = File.AppendText(Errorpath))
                {
                    sw.WriteLine("=============Error Logging ===========");
                    sw.WriteLine("===========Start============= " + DateTime.Now);
                    sw.WriteLine("Error Message: " + e.Message);
                    sw.WriteLine("Stack Trace: " + e.StackTrace);
                    sw.WriteLine("===========End============= " + DateTime.Now);
                }
            }
        }
    }
}
