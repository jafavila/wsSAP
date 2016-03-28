using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsSap.Util
{
  public class CArgPS // JAFF 20160323 - Miercoles argumentos para de un producto solucion
  {
    protected string _credito;
    protected string _sociedad;
    protected string _idps;

    public string Credito { get {return this._credito;} }
    public string Sociedad { get {return this._sociedad;} }
    public string IdPS{ get {return this._idps;} }

    public void setArgPS(string argCredito, string argSociedad, string argIdPS)
    {
      this._credito = argCredito;
      this._sociedad = argSociedad;
      this._idps = argIdPS;
    }

  }
}
