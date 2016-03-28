using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsSap
{

  using ZSolcartera = wsSap.wsSolcartera.ZCMLMF_SOLCARTERA; 
  using ZTsolcartera      = wsSap.wsSolcartera.ZCMLMF_WF_SOLCARTERA_V8;
  using ZSolcarteraCliente = wsSap.wsSolcartera.ZCMLMF_WF_SOLCARTERA_V8Client;
  using ZSolcarteraResponse = wsSap.wsSolcartera.ZCMLMF_SOLCARTERAResponse;
  using ZSolcarteraRequest  = wsSap.wsSolcartera.ZCMLMF_SOLCARTERARequest;
  using ZSolcartera_solicitud = wsSap.wsSolcartera.ZCML_SOLCARTERA;
  using ZSolcartera_detalle   = wsSap.wsSolcartera.ZCML_DETSOLCART;

  public class Solcartera
  {

    #region estatico
    [System.Diagnostics.Conditional("DEBUG")]
    static void dbgErr(object value)
    {
      System.Diagnostics.Debug.WriteLine(value);
    }
    #endregion

    #region privado
    private const string EndpointName = "ZCMLMF_WF_SOLCARTERA_V8";
    private const string User = "procesosp";
    private const string Pass = "PatSap01";
    private void updateStatus(bool argSucc, int argErrno = 0, string argErr = "Init")
    {
      _respuesta.update(argSucc, argErrno, argErr);
    }
    #endregion

    #region protegido
    protected List<ZSolcartera_solicitud> _catSolicitudes = new List<ZSolcartera_solicitud>();
    protected List<ZSolcartera_detalle> _catDetalleSolicitud = new List<ZSolcartera_detalle>();
    protected Respuesta<ZSolcarteraResponse> _respuesta = new Respuesta<ZSolcarteraResponse>();
    #endregion

    #region publico
    public static string testc()
    {
      string lres = "";
      try
      {
        ZSolcarteraCliente lcliente = new ZSolcarteraCliente(Solcartera.EndpointName);

        lcliente.ClientCredentials.UserName.UserName = Solcartera.User;
        lcliente.ClientCredentials.UserName.Password = Solcartera.Pass;
        lcliente.ChannelFactory.Credentials.SupportInteractive = false;

        ZSolcartera lenviar = new ZSolcartera();

        /*Enviar sin datos*/
        lenviar.T_SOLCARTERA = new ZSolcartera_solicitud[1];
        lenviar.T_DETSOLCART = new ZSolcartera_detalle[1];

        /*Datos a enviar*/
        ZSolcartera_solicitud lsol = new ZSolcartera_solicitud();
        ZSolcartera_detalle ldet = new ZSolcartera_detalle();
        lenviar.T_ENT_DETSOLCART = new ZSolcartera_detalle[] { ldet };
        lenviar.T_ENT_SOLCARTERA = new ZSolcartera_solicitud[] { lsol };

        ZSolcarteraResponse lrespuesta = lcliente.ZCMLMF_SOLCARTERA(lenviar);

        if (lrespuesta.ERROR == 1)
        {
          lres = "Solcartera Error == 1";
          Solcartera.dbgErr("WsSolcartera Error == 1");
        }
        else lres = "Esta hecho con exito la solicitud";

      }
      catch (Exception aexception)
      {
        lres ="Exc:"+ aexception.Message;
        System.Diagnostics.Debug.WriteLine("Tetc.Exception: " + aexception.Message);
      }
      return lres;
    }
    public void Limpiar()
    {
      _catSolicitudes.Clear();
      _catSolicitudes.Clear();
    }
    public void addSolicitud(ZSolcartera_solicitud argSol)
    {
      if( argSol != null )
        _catSolicitudes.Add(argSol);
    }
    public void addDetalleSolicitud(ZSolcartera_detalle argSol)
    {
      if( argSol != null )
        _catDetalleSolicitud.Add(argSol);
    }

    public void addDetalleSolicitud(List<ZSolcartera_detalle> argCatDetSol)
    {
      if (argCatDetSol!= null)
        _catDetalleSolicitud.AddRange(argCatDetSol);
    }
    public bool realizarPeticion()
    {
      ZSolcarteraCliente lcliente = null;
      ZSolcartera lenviar = null;
      ZSolcarteraResponse lws_respuesta = null;
      bool lres = false;

      try
      {
        lcliente = new ZSolcarteraCliente(Solcartera.EndpointName);
        lenviar = new ZSolcartera();
        this._respuesta.update(false, 1, "Inicio de la solicitud a SAP");

        lcliente.ClientCredentials.UserName.UserName = Solcartera.User;
        lcliente.ClientCredentials.UserName.Password = Solcartera.Pass;
        lcliente.ChannelFactory.Credentials.SupportInteractive = false;

        /*Enviar sin datos*/
        lenviar.T_SOLCARTERA = new ZSolcartera_solicitud[1];
        lenviar.T_DETSOLCART = new ZSolcartera_detalle[1];

        /*Datos a enviar*/
        lenviar.T_ENT_SOLCARTERA = this._catSolicitudes.ToArray(); ;
        lenviar.T_ENT_DETSOLCART = this._catDetalleSolicitud.ToArray();

        lws_respuesta = lcliente.ZCMLMF_SOLCARTERA(lenviar);

        _respuesta.catRespuesta = lws_respuesta;

        if (lws_respuesta != null && lws_respuesta.ERROR == 1)
        {
          Solcartera.dbgErr("WsSolcartera Error == 1");
          lres = false;
          this.updateStatus(lres, 1, "Solicitud no procesada, error == 1");
        }
        else
        {
          lres = true;
          this.updateStatus(lres, 0, "Esta hecha exitosamente la solicitud a SAP");
        }

      }
      catch (Exception aexception)
      {
        lres = false;
        this.updateStatus(lres, 1, "Anomalia-realizarSolicitud: " + aexception.Message);
      }
      return lres;

    }
    public Respuesta<ZSolcarteraResponse> getRespuesta()
    {
      return this._respuesta;
    }
    public Util.CSuccess getSuccess()
    {
      return _respuesta.Success;
    }
    #endregion

    #region claseInternas
    public class Respuesta<T> : Util.RespuestaWS<T> 
    {

    }

    #endregion

  } 
}
