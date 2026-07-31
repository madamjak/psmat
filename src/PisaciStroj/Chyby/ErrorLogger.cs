using System;
using System.IO;
using System.Text;

namespace PisaciStroj.Chyby
{
    public class Chyba
    {
        public string Parametre { get; set; }

        public Exception Ex { get; set; }
    }

    public class ErrorLogger
    {
        private static ErrorLogger instance = null;
        private static readonly object lockObject = new object();
        private ErrorLogger()
        {
            Konstruktor();
        }

        private void Konstruktor()
        {
            _sb = new StringBuilder();
            _sb.AppendLine(string.Format("LogTime|Data|Message|StackTrace|InnerEx"));
        }

        private StringBuilder _sb;
        private bool _chyba = false;

        public static ErrorLogger GetInstance()
        {
            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = new ErrorLogger();
                }
            }
            return instance;
        }

        public void Log(Chyba chyba)
        {
            _sb.AppendLine(string.Format("{0}|{1}|{2}|{3}|{4}", DateTime.Now.ToString("yyy-MM-dd-HH-mm-ss"), 
                chyba.Parametre, 
                string.Format("\"{0}\"", chyba.Ex.Message), 
                chyba.Ex.StackTrace, 
                chyba.Ex.InnerException));
            _chyba = true;
        }

        public string UlozDoSuboru()
        {
            if (_chyba)
            {
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var cesta = Path.Combine(dir, string.Format("error-{0}.psmerr", DateTime.Now.ToString("yyy-MM-dd-HH-mm-ss")));
                
                using (var writer = new StreamWriter(Path.GetFullPath(cesta)))
                {
                    writer.Write(_sb.ToString());
                }

                return cesta;
            }

            return string.Empty;
        }
    }
}
