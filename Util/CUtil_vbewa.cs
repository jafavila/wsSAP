using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsSap.Util
{
  using CZcmlPabiertas = wsSap.wsPabierta.ZcmlPabiertasV2; // alias

  public class CUtil_vbewa
  {

    #region privado
    private string[] vbewa_accesorios = new string[] { "0110", "0192", "0193", "0194", "0196", "3151", "3160", "3152", "3156", "3153", "0198", "0205", "0205", "0223", "0224", "0225", "3174", "3165", "0112", "3154" };
    private string[] vbewa_amortizacion = new string[] { "0125", "0123" };
    private Calculo _calculos = new Calculo();
    private decimal _montoMora = 0.0M;
    private static List<CZcmlPabiertas> getPartidasVencidas(List<CZcmlPabiertas> argPartidaAbierta)
    {
      DateTime lfehca = DateTime.Now;
      var lvencida = (from a in argPartidaAbierta
                      where !a.Zfbdt.Equals("") && DateTime.Parse(a.Zfbdt) < lfehca
                      select a).ToList();
      return lvencida;
    }

    private bool _mnsGenerada = false;

    #endregion

    #region protegido

    protected List<CZcmlPabiertas> partidasAbiertas
    { set; get; }

    protected decimal sumaAccesorios()
    {
      decimal lres = 0.0M;
      if (this.partidasAbiertas == null)
        return lres;
      int lno_elementos = this.partidasAbiertas.Count;
      CZcmlPabiertas lpa = null;
      for (int va = 0; va < lno_elementos; va++)
      {
        lpa = this.partidasAbiertas[va];
        if (!this.esAccesorio(lpa.Vbewa))
          continue;
        lres += lpa.ImpReev;
      }
      return lres;
    }

    protected decimal sumaAmotizaciones()
    {
      decimal lres = 0.0M;
      if (this.partidasAbiertas == null)
        return lres;
      int lno_elementos = this.partidasAbiertas.Count;
      CZcmlPabiertas lpa = null;
      for (int va = 0; va < lno_elementos; va++)
      {
        lpa = this.partidasAbiertas[va];
        if (!this.esAmortizacion(lpa.Vbewa))
          continue;
        lres += lpa.ImpReev;
      }
      return lres;
    }

    protected decimal caclMensualidad()
    {
      decimal lres = 0.0M;
      if (this.partidasAbiertas == null)
        return lres;
      int lno_elementos = this.partidasAbiertas.Count;
      CZcmlPabiertas lpa = null;
      Decimal mens_actual = 0.0M;
      Decimal mens_anterior = 0.0M;
      Decimal mora_ant = 0.0M;
      Decimal mora_actu = 0.0M;
      DateTime lfcontable = DateTime.MinValue;
      DateTime lahora = DateTime.Now;
      DateTime lanterior = DateTime.Now;

      int anio_ant = lahora.Year;
      int mes_ant = lahora.Month;

      // mes anterior
      {
        if (mes_ant == 1)
        {
          mes_ant = 12;
          anio_ant--;
        }
        else
          mes_ant--;
      }

      for (int va = 0; va < lno_elementos; va++)
      {
        lpa = this.partidasAbiertas[va];
        lfcontable = Convert.ToDateTime(lpa.Budat);

        /*Mes actual*/
        if (lfcontable.Year == lahora.Year && lfcontable.Month == lahora.Month)
        {
          this._mnsGenerada = true;
          /*Obtener de mensualidad*/
          if (lpa.Shkzg.Equals("s") || lpa.Shkzg.Equals("S"))
            mens_actual += lpa.ImpReev;

          /*Obtener mora*/
          if (lpa.Vbewa.Equals("3160"))
            mora_actu = lpa.ImpReev;
        }
          /*Mes anterior*/
        else if (lfcontable.Year == anio_ant && lfcontable.Month == mes_ant)
        {
          /*Obtener de mensualidad*/
          if (lpa.Shkzg.Equals("s") || lpa.Shkzg.Equals("S"))
            mens_anterior += lpa.ImpReev;

          /*Obtener mora*/
          if (lpa.Vbewa.Equals("3160"))
            mora_ant = lpa.ImpReev;
        }
      }
      this.MontoMora = (0.0M < mora_actu ? mora_actu : mora_ant); // JAFF 20160315_2048 - Martes
      lres = (0.0M < mens_actual ? mens_actual : mens_anterior);
      return lres;
    }

    protected bool esAccesorio(string argVbewa)
    {
      return this.vbewa_accesorios.Contains(argVbewa);
    }

    protected bool esAmortizacion(string argVbewa)
    {
      return this.vbewa_amortizacion.Contains(argVbewa);
    }

    protected decimal totalAdecudo()
    {
      decimal lres = 0.0M;

      try
      {
        if (this.partidasAbiertas == null)
          return lres;
        int lno_elementos = this.partidasAbiertas.Count;
        CZcmlPabiertas lpa = null;
        for (int va = 0; va < lno_elementos; va++)
        {
          lpa = this.partidasAbiertas[va];
          if (lpa.Shkzg.Equals("s") || lpa.Shkzg.Equals("S"))
            lres += lpa.ImpReev;
          else if (lpa.Shkzg.Equals("h") || lpa.Shkzg.Equals("H"))
            lres -= lpa.ImpReev;
        }
      }
      catch (Exception aexception)
      {
        lres = 0.0M;
        System.Diagnostics.Debug.WriteLine(aexception.Message);
      }
      return lres;
    }

    #endregion

    #region publico
    public decimal MontoMora { set { this._montoMora = value; } get { return this._montoMora; } }

    public void realizarCalculos(int argDescAcc, int argDescAmor)
    {
      decimal sumAcc = this.sumaAccesorios();
      decimal sumAmot = this.sumaAmotizaciones();
      /* Incorrecto  validacion // JAFF 20160315_2058 - Martes
      decimal quita_directa       = ( argDescAcc == 0 ?  sumAcc : sumAcc  * (argDescAcc  / 100.0M) );
      decimal quita_condificional = ( argDescAmor == 0 ? sumAmot : sumAmot * (argDescAmor / 100.0M) );
       * */

      decimal quita_directa = sumAcc * (argDescAcc / 100.0M);
      decimal quita_condificional = sumAmot * (argDescAmor / 100.0M);

      decimal total_adeudo = this.totalAdecudo();
      decimal mensualidad = this.caclMensualidad();

      this._calculos.setCalculos(quita_directa, quita_condificional, total_adeudo, mensualidad, this.MontoMora,
        this._mnsGenerada );
    }


    public Calculo Calculos { get { return this._calculos; } }

    public CUtil_vbewa(List<CZcmlPabiertas> pa)
    {
      this.partidasAbiertas = pa;
    }
    #endregion

    #region claseinternas

    public class Calculo
    {
      public void setCalculos(decimal argQuita_directa, decimal argQuita_condicional,
        decimal argTotal_adeudo, decimal argMensualidad, decimal argMora, bool argMnsGenerada)
      {
        this.quita_condicional = argQuita_condicional;
        this.quita_directa = argQuita_directa;
        this.total_adeudo = argTotal_adeudo;
        this.mensualidad = argMensualidad;
        this.mora = argMora;
        this.mnsGenerada = argMnsGenerada;
      }
      protected decimal quita_directa = 0.0M;
      protected decimal quita_condicional = 0.0M;
      protected decimal total_adeudo = 0.0M;
      protected decimal mensualidad = 0.0M;
      protected decimal mora = 0.0M;
      protected bool mnsGenerada = false;

      protected decimal getTotalApagr()
      {
        Decimal lres = 0.0M;
        if (this.mnsGenerada)
          lres = this.total_adeudo - this.Total_quita;
        else
          lres = this.total_adeudo - this.Total_quita + this.mensualidad + this.mora;
        return lres;
      }

      public decimal Quita_directa { get { return this.quita_directa; } }
      public decimal Quita_codicional { get { return this.quita_condicional; } }
      public decimal Total_adedudo { get { return this.total_adeudo; } }
      public decimal Total_quita { get { return this.quita_directa + this.quita_condicional; } }
      public decimal Total_pagar { get { return this.getTotalApagr(); } }
      public decimal Mensualidad { get { return this.mensualidad; } }
      public decimal Mora { get { return this.mora; } }

    }

    public class SumaPorDocumento
    {
      List<Documento> _acce = new List<Documento>();
      List<Documento> _amort = new List<Documento>();
      public class Documento
      {
        public string documento { set; get; }
        public Documento(string argDoc)
        {
          this.documento = argDoc;
          this.suma = 0.0M;
        }
        public decimal suma { get; set; }
      }
      public void init()
      {
        string[] vbewa_acce = new String[] { "0110", "0192", "0193", "0194", "0196", "3151", "3160", "3152", "3156", "3153", "0198", "0205", "0205", "0223", "0224", "0225", "3174", "3165", "0112", "3154" };
        int ltam = vbewa_acce.Length;
        Documento ldoc = null;
        for (int va = 0; va < ltam; va++)
        {
          ldoc = new Documento(vbewa_acce[va]);
          _acce.Add(ldoc);
        }

        string[] vbewa_amortizacion = new string[] { "0125", "0123" };

        ltam = vbewa_acce.Length;
        for (int va = 0; va < ltam; va++)
        {
          ldoc = new Documento(vbewa_amortizacion[va]);
          _amort.Add(ldoc);
        }
      }
      private Documento getMov(string strDoc)
      {
        int ltam = this._acce.Count;
        Documento ldoc = null;
        for (int va = 0; va < ltam; va++)
        {
          ldoc = this._acce[va];
          if (ldoc.documento.Equals(strDoc))
            return ldoc;
        }
        return ldoc;
      }

      public void sumar(string strDoc, decimal argValor) // JAFF 20160315_2226 - Martes
      {
        Documento ldoc = null;
        ldoc = this.getDoc(strDoc);
        if (ldoc != null)
          ldoc.suma += argValor;
      }

      private Documento getDoc(string strDoc)
      {
        int ltam = this._acce.Count;
        Documento ldoc = null;
        for (int va = 0; va < ltam; va++)
        {
          ldoc = this._acce[va];
          if (ldoc.documento.Equals(strDoc))
            return ldoc;
        }
        return ldoc;
      }


    }

    #endregion
  }

}
