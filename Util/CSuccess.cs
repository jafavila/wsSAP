using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsSap.Util
{

      public class CSuccess
      {
        protected int    _errorno;
        protected string _error;
        protected bool   _success;

        public int errorno  { get {return this._errorno;} }
        public string error { get { return this._error; } }
        public bool success { get { return this._success;} }

        public void update(bool argSucc, int argErrno = 0, string argErr = "OK" )
        {
          this._success  = argSucc;
          this._errorno  = argErrno;
          this._error    = argErr;
        }
      }
}
