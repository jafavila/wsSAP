using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsSap.Util
{
  public class RespuestaWS<T> 
  {
        private CSuccess _success = new CSuccess();
        public T catRespuesta ;

        public void update(bool argSucc, int argErrno = 0, string argErr = "Init")
        {
          if (this._success != null)
            this._success.update(argSucc, argErrno, argErr);
        }
        public CSuccess Success { get { return this._success; } }
        public bool success { get { return this._success.success; } }

  }
}
