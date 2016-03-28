using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsSap.Util
{

  using Zpabierta         = wsSap.wsPabierta.ZcmlPabiertasV21;
  using Ztpabierta        = wsSap.wsPabierta.ZcmlPabiertasV2;

  using ZSolcartera_solicitud = wsSap.wsSolcartera.ZCML_SOLCARTERA;
  using ZSolcartera_detalle   = wsSap.wsSolcartera.ZCML_DETSOLCART;

  public class ArmarSolCondonacion
  {
    /*Se asignan en el contructor*/
    protected DatoCondonacion  _datosCodonacion           = null;
    protected List<Ztpabierta> _catPabiertaCondonar       = null;
    /*Destinos de la sol a armar*/
    protected List<ZSolcartera_detalle> _catSolDetalle = new List<ZSolcartera_detalle>();
    protected ZSolcartera_solicitud _solCartera        = new ZSolcartera_solicitud();

    /// <summary>
    /// Constructor para inicializar el catalogo de partidas abiertas y los datos de la condonacion
    /// </summary>
    /// <param name="argDatCondonar">Datos de la condonacion</param>
    /// <param name="argPabierta">Partidas abiertas</param>
    public ArmarSolCondonacion(DatoCondonacion argDatCondonar, List<Ztpabierta> argPabierta)
    {
      this._datosCodonacion = argDatCondonar;
      this._catPabiertaCondonar = argPabierta;
    }

    public ArgSolCartera ejecutar()
    {
      ArgSolCartera lres = null;
      try
      {
        lres = new ArgSolCartera();
        this.crearSolDetalle();
        this.crearSolicitud();
        lres.catSolDetalle = _catSolDetalle;
        lres.solCartera = _solCartera;
      }
      catch (Exception aexeption)
      {
        lres = null;
        System.Diagnostics.Debug.WriteLine("ArmarSolCon.ejecutar-exc: " + aexeption.Message);
      }
      return lres;
    }

    private bool crearSolicitud()

    { 
      bool lres = false;
      try
      {
        _solCartera.ZTIPO = "A";
        _solCartera.ZSOLICITUD = Convert.ToString (_datosCodonacion.IdSolicitud );
        _solCartera.ZFEC_SOLIC = Convert.ToString(_datosCodonacion.FechaPromesa); //0xrevisar formato de fecha
        _solCartera.BUKRS = _datosCodonacion.Sociedad;
        _solCartera.RANL = _datosCodonacion.Credito;
        _solCartera.ZFEC_ULTEST = DateTime.Now.ToString() ;  //0xrevisar formato de fecha
        _solCartera.ZUSER = "Dev";
        _solCartera.ZEST_SOLCAR = "1";
        _solCartera.ZPROMESA_PAGO = _datosCodonacion.PromesaPago;
        _solCartera.ZFEC_PROMESA = _datosCodonacion.FechaPromesa.ToString() ; //0xrevisar formato de fecha
        _solCartera.ZMTO_DESC_MAX = _datosCodonacion.MontoDesMax;
        _solCartera.ZMTO_DESC_CONV = _datosCodonacion.MontoDesConvenido;
      }
      catch (Exception aexception)
      {
        System.Diagnostics.Debug.WriteLine(aexception.Message);
        _solCartera = null;
      }
      return lres;
    }

    private bool crearSolDetalle()
    {
      bool lres = false;
      int ltam = 0;
      Ztpabierta lpa = null;
      ZSolcartera_detalle ldetalle= null;
      if (null == _catPabiertaCondonar || 0 > _catPabiertaCondonar.Count)
        return lres;

      _catSolDetalle.Clear();

      ltam=  _catPabiertaCondonar.Count;

      for (int va = 0; va < ltam; va++)
      {
        lpa = _catPabiertaCondonar[va];
        ldetalle = new ZSolcartera_detalle();

        ldetalle.ZTIPO = "A";
        ldetalle.ZSOLICITUD = Convert.ToString ( this._datosCodonacion.IdSolicitud );
        ldetalle.RANL  = this._datosCodonacion.Credito;
        ldetalle.BELNR = lpa.Belnr;
        ldetalle.DMBTR = lpa.Dmbtr;
        ldetalle.WRBTR = lpa.Wrbtr;
        ldetalle.ZEST_DETSOL = this._datosCodonacion.EstatusDelSol_defualt;
        ldetalle.ZIMP_PAR_MONCRED = lpa.ImpReev;

        _catSolDetalle.Add(ldetalle);
      }
      return lres;
    }

    #region claseinternas
    public class DatoCondonacion
    {
      public DatoCondonacion(string argcredito, string argsociedad, string argidPS,
       int argIdSolicitud, 
        decimal argpromesaPago, DateTime argfechaPromesa, decimal argmontoDesMax, decimal argmontoDesConvenido)
      {
        this._credito = argcredito;
        this._sociedad = argsociedad;
        this._idPS = argidPS;
        this._idSolicitud = argIdSolicitud;
        this._promesaPago = argpromesaPago;
        this._fechaPromesa = argfechaPromesa;
        this._montoDesMax = argmontoDesMax;
        this._montoDesConvenido = argmontoDesConvenido;
      }
      protected string _credito;
      protected string _sociedad;
      protected string _idPS;
      protected int _idSolicitud;
      protected decimal _promesaPago;
      protected DateTime _fechaPromesa;
      protected Decimal _montoDesMax;
      protected Decimal _montoDesConvenido;

      public string Credito { get { return this._credito; } }
      public string Sociedad { get { return this._sociedad; } }
      public string IdPS { get { return this._idPS; } }
      public int IdSolicitud{ get { return this._idSolicitud; } }
      public decimal PromesaPago { get { return this._promesaPago; } }
      public DateTime FechaPromesa { get { return this._fechaPromesa; } }
      public Decimal MontoDesMax { get { return this._montoDesMax; } }
      public Decimal MontoDesConvenido { get { return this._montoDesConvenido; } }
      /// <summary>
      /// Valor por defualt es "1"
      /// </summary>
      public string EstatusDelSol_defualt { get { return DatoCondonacion._ZEST_DETSOL; } }

      protected const string _ZEST_DETSOL = "1";
    }
    public class ArgSolCartera
    {
      public List<ZSolcartera_detalle> catSolDetalle{set; get;}
      public ZSolcartera_solicitud solCartera {set; get;}
    }
    #endregion
  }
}
