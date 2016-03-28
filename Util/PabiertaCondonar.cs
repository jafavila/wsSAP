using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsSap.Util
{
  using Ztpabierta = wsSap.wsPabierta.ZcmlPabiertasV2Response;
  using zpabierta  = wsSap.wsPabierta.ZcmlPabiertasV2;

  public class PabiertaCondonar
  {
    private Ztpabierta _lista_pa { set; get; }
    private Decimal _totalCondonacion = 0.0M;
    private string _strSuccess = "";
    protected List<Grupo_Pabierta> _pa_agrupadas { set; get; }
    protected List<zpabierta> _pa_acondonar = new List<zpabierta>();
    protected int _indice_next_grupo = 0;

    #region publico
    public PabiertaCondonar(Ztpabierta argPa, Decimal argTotCondonacion)
    {
      _lista_pa = argPa;
      _totalCondonacion = argTotCondonacion;
    }
    public bool selecionarPartidasCondonar()
    {
      bool lres = false;
      decimal lmonto_condonar = this._totalCondonacion;
      if (this._totalCondonacion <= 0) return lres;

      try
      {
        Grupo_Pabierta lgrupo = null;
        decimal lmonto_del_grupo = 0.0M;

        this.agruparPorFecha();
        for (int va = 0; va < _pa_agrupadas.Count; va++)
        {
          lgrupo = _pa_agrupadas[va];
          lgrupo._montoContodar = lmonto_condonar;
          lmonto_del_grupo = lgrupo.elegirPartidasCondonar();
          lmonto_condonar -= lmonto_del_grupo;

          for (int v = 0; v < lgrupo._lista_pa_condonar.Count; v++)
          {
            this._pa_acondonar.Add(lgrupo._lista_pa_condonar[v]);
          }
          if (lmonto_condonar <= 0)
          {
            lres = true;
            break;
          }
        }

      }
      catch (Exception aexception)
      {
        lres = false;
        _strSuccess = "Exce:" + aexception.Message;
        System.Diagnostics.Debug.Write("Exce:" + aexception.Message);
      }
      return lres;
    }
    public List<zpabierta> getPartidas()
    {
      return this._pa_acondonar;
    }
    public String StrSuccess { get { return _strSuccess; } }
    #endregion

    #region privado
    private void agruparPorFecha()
    {
      if (_lista_pa == null)
        return;
      var resp_ = _lista_pa.TPabiertas.OrderBy(o => o.Zfbdt).ThenBy(o => o.Vbewa).ThenBy(o => o.Belnr).ToList();
      var result = from r in resp_
                   group r by r.Zfbdt into g
                   select g.Select(x => x.Zfbdt);

      List<zpabierta> Respuestafinal = null;
      List<Grupo_Pabierta> lpa_agrupadas = new List<Grupo_Pabierta>();
      Grupo_Pabierta lgrupo = null;

      foreach (IEnumerable<string> r in result)
      {
        Respuestafinal = new List<zpabierta>();
        Respuestafinal.AddRange(resp_.Where(o => o.Zfbdt == r.FirstOrDefault()).ToList());
        lgrupo = new Grupo_Pabierta(Respuestafinal, Respuestafinal.FirstOrDefault().Zfbdt);
        lpa_agrupadas.Add(lgrupo);
      }

      if (this._pa_agrupadas != null)
        this._pa_agrupadas.Clear();
      this._pa_agrupadas = lpa_agrupadas;

      Console.WriteLine("lgrupo con: {0} ", lpa_agrupadas.Count);
    }
    private Grupo_Pabierta next_grupo()
    {
      Grupo_Pabierta lres = null;
      if (null == _pa_agrupadas || 0 == _pa_agrupadas.Count)
        return lres;
      if (_indice_next_grupo < _pa_agrupadas.Count)
      {
        lres = _pa_agrupadas[_indice_next_grupo];
      }
      _indice_next_grupo++;
      return lres;
    }
    #endregion

    #region clase_internas
    public class Grupo_Pabierta
    {
      protected static string[] Prioridades = new string[] { "3165", "4165", "0112", "0192", "0205", "4192", "5192", "0193", "0207", "4193", "5193", "0223", "0224", "0196", "4196", "5196", "0194", "4194", "5194", "3153", "4153", "5153", "3151", "4151", "5151", "3152", "4152", "5152", "3154", "3174", "0110", "116", "0198", "0225", /*>Amortizacion*/ "5123", "5125", "4123", "4125", "0125", "0123" };
      public List<zpabierta> _lista_pabierta { set; get; }
      public List<zpabierta> _lista_pa_condonar = new List<zpabierta>();
      public DateTime fecha { set; get; }
      public string strfecha { set; get; }
      public Decimal _montoContodar = 0.0M;
      protected int _indice_prioridad = 0;

      #region publico
      public Grupo_Pabierta(List<zpabierta> argLista_pa, string argFecha)
      {
        this._lista_pabierta = argLista_pa;
        this.strfecha = argFecha;
      }
      public decimal elegirPartidasCondonar()
      {
        decimal lres = 0.0M;
        bool lcontinuar = true;
        this._lista_pa_condonar.Clear();

        zpabierta pa = null;
        print_grupo(this._lista_pabierta);

        while (lcontinuar)
        {
          if (this._montoContodar <= 0)
          {
            lcontinuar = false;
            break;
          }
          pa = siguiente_pa();
          if (pa == null)
          {
            lcontinuar = false;
            break;
          }

          lres += pa.ImpReev;
          this._montoContodar -= pa.ImpReev;
          if (this._montoContodar < 0)
          {
            decimal ldiff = pa.ImpReev - Math.Abs(this._montoContodar);
            if (pa.Waers.Equals("MXN"))
              pa.ImpReev = ldiff;
            else
            {

              decimal tipoCambio = (pa.Wrbtr / pa.ImpReev);
              pa.ImpReev = tipoCambio * ldiff;
            }
          }
          else
            pa.ImpReev = 0.0M;
          this._lista_pa_condonar.Add(pa);

        }
        return lres;
      }
      #endregion

      #region privado
      private void print_grupo(List<zpabierta> pa)
      {
        if (pa == null)
        {
          Console.WriteLine("Sin pabiertas a imprimir.");
          return;
        }
        for (int va = 0; va < pa.Count; va++)
        {
          Console.WriteLine("{0}\t {1}",
            pa[va].Ranl, pa[va].Vbewa);
        }
      }
      private string siguienteMoviento()
      {
        string lres = null;
        if (_indice_prioridad < Grupo_Pabierta.Prioridades.Length)
        {
          lres = Grupo_Pabierta.Prioridades[_indice_prioridad];
        }
        _indice_prioridad++;
        return lres;
      }
      private zpabierta siguiente_pa()
      {
        zpabierta lres = null;
        string argMov = "";
        bool lcontinuar = true;



        while (lcontinuar)
        {

          argMov = this.siguienteMoviento();
          for (int va = 0; va < _lista_pabierta.Count && lcontinuar; va++)
          {
            if (argMov == null)
            {
              lcontinuar = false;
              return null;
            }
            Console.WriteLine("{0} == {1}", _lista_pabierta[va].Vbewa, argMov);
            if (_lista_pabierta[va].Vbewa.Equals(argMov))
            {
              lres = _lista_pabierta[va];
              lcontinuar = false;
              break;
            }
          }
          if (lres != null || argMov == null)
            lcontinuar = false;
        }

        if (lres != null)
        {
          _lista_pabierta.Remove(lres);
        }
        return lres;
      }
      #endregion

    }
    #endregion

  }

}
