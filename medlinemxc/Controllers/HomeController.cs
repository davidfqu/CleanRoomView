using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IBM.Data.DB2.iSeries;
using System.Data.Odbc;
using System.Data.SqlClient;
using WebMatrix.Data;
using medlinemxc.Models;

namespace medlinemxc.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return RedirectToAction("CleanRoom");
        }
        public ActionResult Video(string id)
        {
            ViewBag.id = id;
            return View();
        }
        public ActionResult Setup()
        {
            return View();
        }

        public ActionResult IndexMDB()
        {
            return View();
        }

        public ActionResult CleanRoom()
        {
            int totalactual = 0; 
            int totalmeta = 0;
            var db = Database.Open("PlayBCon");
            var queryLines = @"select h.linea as linea, h.linea_desc as descripcion
, cast(cast(sum(d.actual)/sum(d.meta)*100 as int) as varchar) + '%' promedio, sum(d.actual) actual, sum(d.meta) meta from t_linehr h 
left join t_linehrdet d on h.folio = d.folio
where cast(h.fecha as date) = cast(GETDATE() as date)
and cast(substring(d.hora, 9,2) as int) <= (datepart (hour, getdate())) 
group by h.folio, h.linea, h.linea_desc
order by cast(h.linea as int)";

            var linesInfo = db.Query(queryLines).ToList();
            int promtotal = 0;

            foreach (var item in linesInfo)
            {
                promtotal = promtotal +Convert.ToInt16(item.promedio.Remove(item.promedio.Length - 1, 1));
                totalactual = totalactual + Convert.ToInt16(Math.Round(item.actual));
                totalmeta = totalmeta + Convert.ToInt16(Math.Round(item.meta));

            }

            ViewBag.Total = Convert.ToString(totalactual) + "/" + Convert.ToString(totalmeta);

            try
            {
                promtotal = promtotal / linesInfo.Count;
            }
            catch
            {
                promtotal = 0;
            }

            string color = "bg-success";

            if (promtotal < 85)
            {
                color = "bg-danger";
            }
            else
            if (promtotal < 95)
            {
                color = "bg-warning";
            }
            else
            {
                color = "bg-success";
            }

            ViewBag.ColorTotal = color;

            ViewBag.linesAvg = Convert.ToString(promtotal) + "%";

            ViewBag.linesInfo = linesInfo;



            return View();
        }

        public ActionResult ProdDiariaCR(string fecha = null)
        {
            var db = Database.Open("PlayBCon");
            var queryLines = "";
            var queryTotal = "";

            if(fecha == null)
            {
                 queryLines = @"Select f1.folio as folio, f1.linea as linea , f1.descr as descripcion , f1.meta_actual as meta_actual, f1.prod_actual as prod_actual, f1.defectos_actual as defectos_actual, f2.line_downs as line_downs
from (select  h.folio as folio, h.linea as linea, h.linea_desc as descr, sum(d.meta) as meta_actual, sum(d.actual) as  prod_actual, sum(d.defectos) as defectos_actual
from t_linehr h 
left join t_linehrdet d on h.folio = d.folio
where cast(h.fecha as date) = cast(GETDATE() as date)
and cast(substring(d.hora, 9,2) as int) <= (datepart (hour, getdate())) 
group by h.folio, h.linea, h.linea_desc
--order by cast(h.linea as int)
) as f1
inner join
(select  h.folio as folio, h.linea as linea, count(ld.folio) as line_downs from t_linehr h
left join t_linedown ld on h.linea = ld.linea and h.fecha = ld.fecha
where cast(h.fecha as date) = cast(GETDATE() as date)
group by h.folio, h.linea) as f2
on f1.folio = f2.folio and f1.linea = f2.linea
order by cast(f1.linea as int)";

                queryTotal = @"Select f1.fecha, sum(f1.meta_actual) as meta_actual, sum(f1.prod_actual) as prod_actual, sum(f1.defectos_actual) as defectos_actual, sum(f2.line_downs) as line_downs
from (select  h.fecha as fecha, h.folio as folio, h.linea as linea, h.linea_desc as descr, sum(d.meta) as meta_actual, sum(d.actual) as  prod_actual, sum(d.defectos) as defectos_actual
from t_linehr h 
left join t_linehrdet d on h.folio = d.folio
where cast(h.fecha as date) = cast(GETDATE() as date)
and cast(substring(d.hora, 9,2) as int) <= (datepart (hour, getdate())) 
group by h.folio, h.linea, h.linea_desc, h.fecha
--order by cast(h.linea as int)
) as f1
inner join
(select  h.folio as folio, h.linea as linea, count(ld.folio) as line_downs from t_linehr h
left join t_linedown ld on h.linea = ld.linea and h.fecha = ld.fecha
where cast(h.fecha as date) = cast(GETDATE() as date)
group by h.folio, h.linea) as f2
on f1.folio = f2.folio and f1.linea = f2.linea
group by f1.fecha";

                ViewBag.fecha = null;
                ViewBag.actual = 1;
            }
            else
            {
                queryLines = @"Select f1.folio as folio, f1.linea as linea , f1.descr as descripcion , f1.meta_actual as meta_actual, f1.prod_actual as prod_actual, f1.defectos_actual as defectos_actual, f2.line_downs as line_downs
from (select  h.folio as folio, h.linea as linea, h.linea_desc as descr, sum(d.meta) as meta_actual, sum(d.actual) as  prod_actual, sum(d.defectos) as defectos_actual
from t_linehr h 
left join t_linehrdet d on h.folio = d.folio
where cast(h.fecha as date) = '" + fecha + @"'
group by h.folio, h.linea, h.linea_desc
--order by cast(h.linea as int)
) as f1
inner join
(select  h.folio as folio, h.linea as linea, count(ld.folio) as line_downs from t_linehr h
left join t_linedown ld on h.linea = ld.linea and h.fecha = ld.fecha
where cast(h.fecha as date) = '" + fecha + @"'
group by h.folio, h.linea) as f2
on f1.folio = f2.folio and f1.linea = f2.linea
order by cast(f1.linea as int)";

                queryTotal = @"Select f1.fecha, sum(f1.meta_actual) as meta_actual, sum(f1.prod_actual) as prod_actual, sum(f1.defectos_actual) as defectos_actual, sum(f2.line_downs) as line_downs
from (select  h.fecha as fecha, h.folio as folio, h.linea as linea, h.linea_desc as descr, sum(d.meta) as meta_actual, sum(d.actual) as  prod_actual, sum(d.defectos) as defectos_actual
from t_linehr h 
left join t_linehrdet d on h.folio = d.folio
where cast(h.fecha as date) = '" + fecha + @"'
group by h.folio, h.linea, h.linea_desc, h.fecha
--order by cast(h.linea as int)
) as f1
inner join
(select  h.folio as folio, h.linea as linea, count(ld.folio) as line_downs from t_linehr h
left join t_linedown ld on h.linea = ld.linea and h.fecha = ld.fecha
where cast(h.fecha as date) = '" + fecha + @"'
group by h.folio, h.linea) as f2
on f1.folio = f2.folio and f1.linea = f2.linea
group by f1.fecha";

                ViewBag.fecha = fecha;
                ViewBag.actual = 0;
            }
           

            var linesInfo = db.Query(queryLines).ToList();
            var linesTotal = db.Query(queryTotal);

            string rojo = "'#de5f5f'";
            string verde = "'#7dc975'";
            string data1Graph = "[";
            string data2Graph = "[";
            string colorGraph = "[";
            string xAxisGraph = "[";
            int n = 0;

            foreach (var item in linesInfo)
            {

                n++;
                data1Graph = data1Graph + Convert.ToString(item.prod_actual);
                data2Graph = data2Graph + Convert.ToString(item.meta_actual);
                xAxisGraph = xAxisGraph + Convert.ToString(item.linea);

                if (item.prod_actual < item.meta_actual)
                    colorGraph = colorGraph + rojo;
                    else
                    colorGraph = colorGraph + verde;
                

                if (n == linesInfo.Count())
                {
                    data1Graph = data1Graph + "]";
                    data2Graph = data2Graph + "]";
                    colorGraph = colorGraph + "]";
                    xAxisGraph = xAxisGraph + "]";
                }
                else
                {
                    data1Graph = data1Graph + ",";
                    data2Graph = data2Graph + ",";
                    colorGraph = colorGraph + ",";
                    xAxisGraph = xAxisGraph + ",";
                }

            }


            try
            {
                ViewBag.metaTotal = linesTotal.ElementAt(0)[1];
                ViewBag.cantTotal = linesTotal.ElementAt(0)[2];
                ViewBag.defectosTotal = linesTotal.ElementAt(0)[3];
                ViewBag.lineDownsTotal = linesTotal.ElementAt(0)[4];
            }
            catch
            {
                ViewBag.metaTotal = "0";
                ViewBag.cantTotal = "0";
                ViewBag.defectosTotal = "0";
                ViewBag.lineDownsTotal = "0";
            }
                
            
            ViewBag.data1Graph = data1Graph;
            ViewBag.data2Graph = data2Graph;
            ViewBag.colorGraph = colorGraph;
            ViewBag.xAxisGraph = xAxisGraph;
           

            ViewBag.linesInfo = linesInfo;


            return View();
        }

        public ActionResult ProdDiariaCR2()
        {
            return View();
        }

        public ActionResult mtv()
        {
            return View();
        }


        public ActionResult Horaxhora(int id =1, string fecha = null)
        {
            var db = Database.Open("PlayBCon");
            var queryHeader = "";
            var queryGraph = "";

            if(fecha == null)
            {
                queryHeader = @"Select f1.linea as linea , f1.descr as descripcion , cast(f1.prod_actual as varchar) + '/' + cast(f1.meta_actual as varchar) meta ,  f1.defectos_actual as defectos,
cast(cast(f1.prod_actual/f1.meta_actual *100 as int) as varchar) +'%' promedio, f2.line_downs as line_downs
from (select  h.folio as folio, h.linea as linea, h.linea_desc as descr, sum(d.meta) as meta_actual, sum(d.actual) as  prod_actual, sum(d.defectos) as defectos_actual
from t_linehr h 
left join t_linehrdet d on h.folio = d.folio
where cast(h.fecha as date) = cast(GETDATE() as date) and h.linea = " + id.ToString() + @"
and cast(substring(d.hora, 9,2) as int) <= (datepart (hour, getdate())) 
group by h.folio, h.linea, h.linea_desc
--order by cast(h.linea as int)
) as f1
inner join
(select  h.folio as folio, h.linea as linea, count(ld.folio) as line_downs from t_linehr h
left join t_linedown ld on h.linea = ld.linea and h.fecha = ld.fecha
where cast(h.fecha as date) = cast(GETDATE() as date) and h.linea = " + id.ToString() + @"
group by h.folio, h.linea) as f2
on f1.folio = f2.folio and f1.linea = f2.linea
order by cast(f1.linea as int)";

                queryGraph = @"select  h.linea, substring(d.hora, 8,6)  as hora, d.consec, d.actual as actual_hora, d.meta as meta_hora
from t_linehr h 
inner join t_linehrdet d on h.folio = d.folio
where h.linea = " + id.ToString() + " and cast(h.fecha as date) = cast(GETDATE() as date)order by d.consec";

            }
            else
            {
                queryHeader = @"Select f1.linea as linea , f1.descr as descripcion , cast(f1.prod_actual as varchar) + '/' + cast(f1.meta_actual as varchar) meta ,  f1.defectos_actual as defectos,
cast(cast(f1.prod_actual/f1.meta_actual *100 as int) as varchar) +'%' promedio, f2.line_downs as line_downs
from (select  h.folio as folio, h.linea as linea, h.linea_desc as descr, sum(d.meta) as meta_actual, sum(d.actual) as  prod_actual, sum(d.defectos) as defectos_actual
from t_linehr h 
left join t_linehrdet d on h.folio = d.folio
where h.fecha  = '" + fecha+ @"' and h.linea = " + id.ToString() + @"
group by h.folio, h.linea, h.linea_desc
--order by cast(h.linea as int)
) as f1
inner join
(select  h.folio as folio, h.linea as linea, count(ld.folio) as line_downs from t_linehr h
left join t_linedown ld on h.linea = ld.linea and h.fecha = ld.fecha
where h.fecha =  '" + fecha + @"' and  h.linea = " + id.ToString() + @"
group by h.folio, h.linea) as f2
on f1.folio = f2.folio and f1.linea = f2.linea
order by cast(f1.linea as int)";

                queryGraph = @"select  h.linea, substring(d.hora, 8,6)  as hora, d.consec, d.actual as actual_hora, d.meta as meta_hora
from t_linehr h 
inner join t_linehrdet d on h.folio = d.folio
where h.linea =  " + id.ToString() + @" and h.fecha = '" + fecha + @"' order by d.consec";

            }


            var headerInfo = db.Query(queryHeader);
            var graphInfo = db.Query(queryGraph).ToList();
            var horactual = System.DateTime.Now.Hour;

            string rojo = "'#de5f5f'";
            string verde = "'#7dc975'";
            string azul = "'#5e9cf2'";
            string data1Graph = "[";
            string data2Graph = "[";
            string colorGraph = "[";
            string xAxisGraph = "[";
            int n = 0;
            foreach(var item in graphInfo)
            {

                n++;
                data1Graph = data1Graph + Convert.ToString(item.actual_hora);
                data2Graph = data2Graph + Convert.ToString(item.meta_hora);
                xAxisGraph = xAxisGraph + "'"+Convert.ToString(item.hora)+"'";

                if(item.actual_hora != null)
                {
                    if(item.consec == horactual-5)
                    {
                        colorGraph = colorGraph + azul;
                    }
                    else
                    {
                        if (item.actual_hora < item.meta_hora)
                        {
                            colorGraph = colorGraph + rojo;
                        }
                        else
                        {
                            colorGraph = colorGraph + verde;
                        }
                    }
                    
                }
                

                if (n == graphInfo.Count())
                {

                    data1Graph = data1Graph + "]";
                    data2Graph = data2Graph + "]";
                    colorGraph = colorGraph + "]";
                    xAxisGraph = xAxisGraph + "]";
                }
                else
                {
                    data1Graph = data1Graph + ",";
                    data2Graph = data2Graph + ",";
                    colorGraph = colorGraph + ",";
                    xAxisGraph = xAxisGraph + ",";

                }

            }
            

            string nombrelinea = "L" + id.ToString();

            ViewBag.xAxisGraph = xAxisGraph;
            ViewBag.colorGraph = colorGraph;
            ViewBag.dataset1 = data1Graph;
            ViewBag.dataset2 = data2Graph;
            ViewBag.nombrelinea = nombrelinea;
            try
            {
                ViewBag.meta = headerInfo.ElementAt(0)[2];
                ViewBag.tipo = headerInfo.ElementAt(0)[1];
                ViewBag.defectos = headerInfo.ElementAt(0)[3];
                ViewBag.promedio = headerInfo.ElementAt(0)[4];
                ViewBag.linedowns = headerInfo.ElementAt(0)[5];
            }
            catch
            {
                ViewBag.meta = "0";
                ViewBag.tipo = "0";
                ViewBag.defectos = "0";
                ViewBag.promedio = "0";
                ViewBag.linedowns = "0";
            }

            return View();
        }


        public ActionResult Efficiency()
        {
            /*
            OdbcConnection myCon = new OdbcConnection();
            string sConnection = "Driver=iSeries Access ODBC Driver;System=AS400SYS;uid=inqmex;pwd=inqmex;";
            myCon.ConnectionString = sConnection;
            try
            {
                myCon.Open();
                Console.WriteLine("Connection successful!");

                string sQuery = "SELECT FKITMSTR.IMPN, FKITMSTR.IMDSC, FKITMSTR.IMCO, FKITMSTR.IMSTCK " +
                    "FROM B20E386T.KBM400MFG.FKITMSTR FKITMSTR " +
                    "WHERE(FKITMSTR.IMCO = 686) AND(FKITMSTR.IMPN LIKE 'DYN%')";
                OdbcCommand myCom = new OdbcCommand(sQuery, myCon);
                OdbcDataReader myReader;
                myReader = myCom.ExecuteReader();
                while (myReader.Read())
                {
                    Console.WriteLine(myReader[0].ToString() + myReader[1].ToString() + myReader[2].ToString() + " " + myReader[3].ToString());
                }
            }
            catch (OdbcException ex)
            {
                Console.WriteLine(ex.Message); return View();
            }
            myCon.Close();
            Console.Read();
            */
            return View();
        }

    }
}