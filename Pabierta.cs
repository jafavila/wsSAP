using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wsSap.Util;

namespace wsSap
{

  using Zpabierta         = wsSap.wsPabierta.ZcmlPabiertasV21;
  using Ztpabierta        = wsSap.wsPabierta.ZcmlPabiertasV2;
  using ZpabiertaCliente  = wsSap.wsPabierta.zcml_pabiertas2_v4Client;
  using ZpabiertaResponse = wsSap.wsPabierta.ZcmlPabiertasV2Response;
  using ZpabiertaRequest  = wsSap.wsPabierta.ZcmlPabiertasV2Request;
  using System.ServiceModel;

    public class Pabierta
    {

     
    private const string EndpointName = "ZCML_PABIERTAS2_V4";
    private const string _userName = "procesosp";
    private const string _password = "PatSap01";
    protected Respuesta<ZpabiertaResponse>  _respuesta = new Respuesta<ZpabiertaResponse>();


    public Pabierta(string argCredito, string argSociedad)
    {
      this.Bukrs = argSociedad;
      this.Ranl = argCredito;
    }

    public List<Ztpabierta> getPartidasCondonar(decimal argMontoCondonar)
    {
      List<Ztpabierta> lres = null;

      if (0 >= argMontoCondonar)
      { 
        this.updateStatus(false, 1, "Monto a condonar debe ser mayor a 0.0");
        return lres;
      }

      if (this._respuesta.catRespuesta == null)
      {
        this.updateStatus(false, 1, "No hay partidiciones abiertas");
        return lres;
      }
      wsSap.Util.PabiertaCondonar lpaCondonar = new wsSap.Util.PabiertaCondonar(this._respuesta.catRespuesta, argMontoCondonar);

      if (lpaCondonar.selecionarPartidasCondonar())
      {
        lres = lpaCondonar.getPartidas();
        this.updateStatus(true, 0, "OK partidas a condonar obtenidas");
      }
      else
      {
        lres = null;
        this.updateStatus(false , 0, "getPartidasCondonar:"+lpaCondonar.StrSuccess);
      }
      return lres;
    }


    private void updateStatus(bool argSucc, int argErrno = 0, string argErr = "Init")
    {
      _respuesta.update(argSucc, argErrno, argErr);
    }

      public bool realizarPeticion()
      {
        bool lres = false;
        if( _respuesta == null ) return lres;

        _respuesta.update( false );
        ZpabiertaCliente paCliente      = null;
        Zpabierta pabierta              = null;
        ZpabiertaResponse paRespuesta   = null;

        try
        {
          _respuesta.update(false, 1, "Iniciando peticion de partidas abiertas.");
          paCliente = new ZpabiertaCliente( Pabierta.EndpointName);
          /*Autenticacion*/

          paCliente.ClientCredentials.UserName.UserName = Pabierta._userName;
          paCliente.ClientCredentials.UserName.Password = Pabierta._password;
          paCliente.ChannelFactory.Credentials.SupportInteractive = false;

          /*Argumentos del ws*/
          pabierta            = new Zpabierta();
          pabierta.PRanl      = _respuesta.ranl;
          pabierta.PBukrs     = _respuesta.bukrs;
          pabierta.TPabiertas = new Ztpabierta[1];

          /*Ejecutar el ws*/
          paRespuesta =  paCliente.ZcmlPabiertasV2( pabierta);

           _respuesta.catRespuesta = paRespuesta;

           if (paRespuesta != null && paRespuesta.TPabiertas.Length > 0)
           {
             lres = true;
             _respuesta.update(true, 0, "OK: Partidas abiertas obtenidas");
           }
           else
           {
             _respuesta.update(false, 1, "No hay partidas abiertas");
             lres = false;
           }

          paCliente.Close();

        }
        catch (Exception aexception)
        {
          _respuesta.update(false, 1, aexception.Message);
        }
        return lres;
      }

      public Respuesta<ZpabiertaResponse> getRespuesta()
      {
        return this._respuesta;
      }

      public CSuccess getSuccess()
      {
        return this._respuesta.Success;
      }

      public string Ranl  { set{this._respuesta.ranl  = value;} get{ return this._respuesta.ranl; } }
      public string Bukrs { set{this._respuesta.bukrs = value;} get{ return this._respuesta.bukrs;} }
      
      public List<Ztpabierta> getPabiertas()
      {
        List<Ztpabierta> lres = null;
        if( this._respuesta.catRespuesta != null &&  this._respuesta.catRespuesta.TPabiertas != null )
          lres = this._respuesta.catRespuesta.TPabiertas.ToList<Ztpabierta>();
        return lres;
      }

      #region claseInternas
      public class Respuesta<T> : RespuestaWS<T>
      {
        public string ranl;
        public string bukrs;
        /*
        private CSuccess _success = new CSuccess();
        public string ranl { set; get; }
        public string bukrs { set; get; }
        public List<Ztpabierta> catPartidaAbieta  = new List<Ztpabierta>();
        public void update(bool argSucc, int argErrno = 0, string argErr = "Init")
        {
          if (this._success != null)
            this._success.update(argSucc, argErrno, argErr);
        }
        public CSuccess Success { get { return this._success; } }
        public bool success { get { return this._success.success; } }

        public  void printResultado()
        {
          if (this.success)
          {
            Console.WriteLine("CResPA is OK. No. Pabiertas:" + this.catPartidaAbieta.Count);
          }
          else { 
            Console.WriteLine("CResPA is OK. "+ _success.error );
          }
        }
        */

      }

     #endregion

    }
}
