using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using medlinemxc.Models;
using System.Data.Odbc;

namespace medlinemxc.Controllers
{
    public class t_boxingscanController : Controller
    {
        private LayoutSystemEntities db = new LayoutSystemEntities();

        // GET: t_boxingscan
        public ActionResult Index()
        {
            return View(db.t_boxingscan.ToList());
        }

        // GET: t_boxingscan/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_boxingscan t_boxingscan = db.t_boxingscan.Find(id);
            if (t_boxingscan == null)
            {
                return HttpNotFound();
            }
            return View(t_boxingscan);
        }

        // GET: t_boxingscan/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: t_boxingscan/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "folio,lote,wo,pn,cajas_total,cajas_scan,fecha")] t_boxingscan t_boxingscan)
        {
            if (ModelState.IsValid)
            {
                db.t_boxingscan.Add(t_boxingscan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(t_boxingscan);
        }

        // GET: t_boxingscan/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_boxingscan t_boxingscan = db.t_boxingscan.Find(id);
            if (t_boxingscan == null)
            {
                return HttpNotFound();
            }
            return View(t_boxingscan);
        }

        // POST: t_boxingscan/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "folio,lote,wo,pn,cajas_total,cajas_scan,fecha")] t_boxingscan t_boxingscan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(t_boxingscan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(t_boxingscan);
        }

        // GET: t_boxingscan/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_boxingscan t_boxingscan = db.t_boxingscan.Find(id);
            if (t_boxingscan == null)
            {
                return HttpNotFound();
            }
            return View(t_boxingscan);
        }

        // POST: t_boxingscan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            t_boxingscan t_boxingscan = db.t_boxingscan.Find(id);
            db.t_boxingscan.Remove(t_boxingscan);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult ScanView(string linea)
        {
            ViewBag.linea = linea;
            string tlinea = db.t_lineconfd.Where(x => x.line == linea).First().clave;
            string enmeta = "0";
            var tablaprogreso = db.sp_dailyline(linea, "1", System.DateTime.Now).OrderBy(x=>x.consec).ToList();
            decimal metaactual = 50000;
            decimal cajasactual = 50000;
            decimal tcajas, tmeta;
            int tmuerto, tkits;
            var config = db.t_config.Find("686");
            var treshold = config.threshold_boxscan;

            int consecactual = 50000;

            tcajas = tablaprogreso.Sum(x => x.cajas); 
            tmeta = Convert.ToDecimal(tablaprogreso.Sum(x => x.meta));
            tkits = tablaprogreso.Sum(x => x.kits);
            tmuerto = tablaprogreso.Sum(x => x.tmuerto);


            int hactual = System.DateTime.Now.Hour - 1;

            foreach(var item in tablaprogreso)
            {
                if(hactual == Convert.ToInt16(item.hora.Substring(0,2)))
                {
                    consecactual = Convert.ToInt16(item.consec);
                    tcajas = tablaprogreso.Where(x => x.consec <= item.consec).Sum(x => x.cajas);
                    tmuerto = tablaprogreso.Where(x => x.consec <= item.consec).Sum(x => x.tmuerto);
                    tkits = tablaprogreso.Where(x => x.consec <= item.consec).Sum(x => x.kits);
                    metaactual = Convert.ToDecimal(item.meta_ac);
                }
                
            }

            if(metaactual == 50000)
            {
                metaactual = tmeta;
            }

            cajasactual = tcajas;

            if (metaactual <= cajasactual)
            {
                enmeta = "1";
            }

            // var metah = db.t_lineconfh.Where(x => x.clave == tlinea).OrderBy(x=>x.consec).ToList();
            //var cajash = db.t_boxingscan_h.Where(x => x.linea == linea && DbFunctions.TruncateTime(x.fecha) >= DateTime.Today).OrderBy(x => x.hora).ToList();
            string ultimolote;
            try
            {
                var queryultimofolios = db.t_boxingscan.Where(x => x.linea == linea).OrderByDescending(x => x.folio).First<t_boxingscan>();
                    ultimolote = queryultimofolios.lote;
            }
            catch
            {
                ultimolote = "0";
            }

           var usuarios = db.t_boxingscan_pass.ToList();

            ViewBag.usuarios = usuarios;
            ViewBag.ultimolote = ultimolote;
            ViewBag.factor_kitsxcaja = config.factor_kitsxcaja;
            ViewBag.tkits = tkits;
            ViewBag.tcajas = tcajas;
            ViewBag.tmeta = metaactual;
            ViewBag.ttmuerto = tmuerto;
            ViewBag.tablaprogreso = tablaprogreso;
            ViewBag.enmeta = enmeta;
            ViewBag.consecactual = consecactual;
            ViewBag.treshold = treshold;
            return View();
        }

        public ActionResult loteScan(string id, string linea, int treshold)
        {
           
            t_boxingscan queryultimofolio = new t_boxingscan();
            
            var queryultimofolios = db.t_boxingscan.Where(x => x.linea == linea).OrderByDescending(x => x.folio).ToList();

            if(queryultimofolios.Any())
            {
                queryultimofolio = queryultimofolios.First<t_boxingscan>();
            }
            else
            {
                queryultimofolio.lote = "0";
            }     
            
                
            string[] infoScan = new string[8];
            t_boxingscan newboxingscan = new t_boxingscan();

            //si el ultimo lote es el mismo no ir a la base de datos de as400
            if (id != queryultimofolio.lote)
            {
                string[] infoLote = new string[5];
                infoScan[0] = "1";
                // var ddlUsuarios = db.t_usuarios.Where(x => x.usuario == id).ToList();
                OdbcConnection myCon = new OdbcConnection();
                string sConnection = "Driver=iSeries Access ODBC Driver;System=AS400SYS;uid=inqmex;pwd=inqmex;";
                myCon.ConnectionString = sConnection;
                try
                {
                    myCon.Open();
                    Console.WriteLine("Connection successful!");

                    string sQuery = "SELECT FCSTRHDR.SHWONO WO, FCSTRHDR.SHLOT LOT, FCSTRHDR.SHSPN PN, FCSTRHDR.SHSQTY CASES, FKITMSTR.IMSTCK KPC FROM B20E386T.CSM400MFG.FCSTRHDR FCSTRHDR INNER JOIN  B20E386T.KBM400MFG.FKITMSTR FKITMSTR ON FCSTRHDR.SHSPN = FKITMSTR.IMPN AND FCSTRHDR.SHSCO = FKITMSTR.IMCO WHERE(FCSTRHDR.SHLOT = '"+id+"' AND FCSTRHDR.SHSCO = '686')";


                    OdbcCommand myCom = new OdbcCommand(sQuery, myCon);
                    OdbcDataReader myReader;
                    myReader = myCom.ExecuteReader();
                    while (myReader.Read())
                    {
                        infoLote[0] = myReader[0].ToString();
                        infoLote[1] = myReader[1].ToString();
                        infoLote[2] = myReader[2].ToString();
                        infoLote[3] = myReader[3].ToString();
                        infoLote[4] = myReader[4].ToString();
                        //Console.WriteLine(myReader[0].ToString() + myReader[1].ToString() + myReader[2].ToString() + " " + myReader[3].ToString());
                    }
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine(ex.Message); return View();
                }
                myCon.Close();
                newboxingscan.folio = queryultimofolio.folio + 1;
                newboxingscan.wo = infoLote[0].Replace(" ", String.Empty);
                newboxingscan.lote = infoLote[1].Replace(" ", String.Empty);
                newboxingscan.pn = infoLote[2].Replace(" ", String.Empty);
                newboxingscan.cajas_total = Convert.ToInt16(infoLote[3]);
                newboxingscan.cajas_scan = Convert.ToDecimal(1.0 / Convert.ToInt16(infoLote[4]));
                newboxingscan.kits_por_caja = Convert.ToInt16(infoLote[4]);
                newboxingscan.fecha = System.DateTime.Now;
                newboxingscan.linea = linea;
            }
            else
            {
                infoScan[0] = "0";
                newboxingscan.folio = queryultimofolio.folio + 1;
                newboxingscan.wo = queryultimofolio.wo;
                newboxingscan.lote = queryultimofolio.lote;
                newboxingscan.pn = queryultimofolio.pn;
                newboxingscan.cajas_total = queryultimofolio.cajas_total;
                newboxingscan.cajas_scan = queryultimofolio.cajas_scan;
                newboxingscan.fecha = System.DateTime.Now;
                newboxingscan.linea = linea;
                newboxingscan.kits_por_caja = queryultimofolio.kits_por_caja;
            }

            //tiempomuerto
            int tiempomuerto = 0;
            var fechacomparar = queryultimofolio.fecha;

            if (queryultimofolio.lote == "0" )// si no hubo ningun registro de esta linea antes
            {
                fechacomparar = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, 6, 0, 0);
            }
            else 
            {
                if(queryultimofolio.fecha.Value.Date != DateTime.Now.Date) // si el ultimo escaneo no fue hoy
                {
                    fechacomparar = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, 6, 0, 0); // hora de inicio de turno
                }
            }

            TimeSpan diff = Convert.ToDateTime(newboxingscan.fecha) - Convert.ToDateTime(fechacomparar);
            double diffm = diff.TotalMinutes;
            int diffms = Convert.ToInt32(Math.Floor(diffm));

            if (queryultimofolio.lote != id)
            {
                tiempomuerto = diffms;
            }
            else
            {
                if(diffms >= treshold)
                {
                    tiempomuerto = diffms;
                }
            }

            

            newboxingscan.tmuerto = tiempomuerto;

           

            double totalkits = Math.Round(Convert.ToDouble(newboxingscan.cajas_total * newboxingscan.kits_por_caja));
            infoScan[1] = newboxingscan.wo;
            infoScan[2] = Convert.ToString(newboxingscan.cajas_total);
            infoScan[3] = Convert.ToString(totalkits);

            int sumkits = db.t_boxingscan.Where(x => x.wo == newboxingscan.wo && x.linea == linea).Count() + 1;
            decimal sumcajas = Math.Round(Convert.ToDecimal(sumkits * newboxingscan.cajas_scan), 2);
            infoScan[6] = Convert.ToString(Math.Round(Convert.ToDouble((sumcajas/newboxingscan.cajas_total)*100))) + "%";

            infoScan[5] = Convert.ToString(sumkits);
            infoScan[4] = Convert.ToString(sumcajas);
           if(sumkits > totalkits)
            {
                newboxingscan.wo_completa = "1";
                infoScan[7] = "1";
                infoScan[6] = "100%";
                infoScan[4] = infoScan[2];
                infoScan[5] = infoScan[3];
            }
           else
            {
                newboxingscan.wo_completa = "0";
                infoScan[7] = "0";
            }

            if (ModelState.IsValid)
            {
                db.t_boxingscan.Add(newboxingscan);
                db.SaveChanges();
            }
            return Json(infoScan, JsonRequestBehavior.AllowGet);

        }

        public string Desbloqueo(string linea, string contrasena)
        {
            string correcto = "0";
            var usuarios = db.t_boxingscan_pass.ToList();
            foreach (var usuario in usuarios)
            {
                if (contrasena == usuario.contrasena)
                {
                    correcto = "1";
                    int ultimobloqueo = 0;
                    try
                    {
                        ultimobloqueo = db.t_boxingscan_cambiowo.OrderByDescending(x => x.folio).ToList().First().folio;
                    }
                    catch
                    {
                        ultimobloqueo = 0;
                    }
                   
                    t_boxingscan_cambiowo newboxingscan_cambiowo = new t_boxingscan_cambiowo();

                    newboxingscan_cambiowo.folio = ultimobloqueo + 1;
                    newboxingscan_cambiowo.fecha = System.DateTime.Now;
                    newboxingscan_cambiowo.bloqueo = 0;
                    newboxingscan_cambiowo.linea = linea;
                    newboxingscan_cambiowo.usuario = usuario.usuario;

                    db.t_boxingscan_cambiowo.Add(newboxingscan_cambiowo);
                    db.SaveChanges();

                    break;
                }
                    
            }

            return correcto;
        }

        public void Bloqueo(string linea, string lote_anterior, string lote_nuevo)
        {
            string wo_anterior = "-", wo_nueva = "-";

            #region as400

            OdbcConnection myCon = new OdbcConnection();
            string sConnection = "Driver=iSeries Access ODBC Driver;System=AS400SYS;uid=inqmex;pwd=inqmex;";
            myCon.ConnectionString = sConnection;

            try
            {
                myCon.Open();
                Console.WriteLine("Connection successful!");

                string sQuery = "SELECT FCSTRHDR.SHWONO WO, FCSTRHDR.SHLOT LOT, FCSTRHDR.SHSPN PN, FCSTRHDR.SHSQTY CASES, FKITMSTR.IMSTCK KPC FROM B20E386T.CSM400MFG.FCSTRHDR FCSTRHDR INNER JOIN  B20E386T.KBM400MFG.FKITMSTR FKITMSTR ON FCSTRHDR.SHSPN = FKITMSTR.IMPN AND FCSTRHDR.SHSCO = FKITMSTR.IMCO WHERE(FCSTRHDR.SHLOT = '" + lote_anterior + "' AND FCSTRHDR.SHSCO = '686')";
                string sQuery2 = "SELECT FCSTRHDR.SHWONO WO, FCSTRHDR.SHLOT LOT, FCSTRHDR.SHSPN PN, FCSTRHDR.SHSQTY CASES, FKITMSTR.IMSTCK KPC FROM B20E386T.CSM400MFG.FCSTRHDR FCSTRHDR INNER JOIN  B20E386T.KBM400MFG.FKITMSTR FKITMSTR ON FCSTRHDR.SHSPN = FKITMSTR.IMPN AND FCSTRHDR.SHSCO = FKITMSTR.IMCO WHERE(FCSTRHDR.SHLOT = '" + lote_nuevo + "' AND FCSTRHDR.SHSCO = '686')";

                OdbcCommand myCom = new OdbcCommand(sQuery, myCon);
                OdbcDataReader myReader;
                myReader = myCom.ExecuteReader();
                while (myReader.Read())
                {
                    wo_anterior = myReader[0].ToString();
                    //Console.WriteLine(myReader[0].ToString() + myReader[1].ToString() + myReader[2].ToString() + " " + myReader[3].ToString());
                }

                OdbcCommand myCom2 = new OdbcCommand(sQuery2, myCon);
                OdbcDataReader myReader2;
                myReader2 = myCom2.ExecuteReader();
                while (myReader2.Read())
                {
                    wo_nueva = myReader2[0].ToString();
                    //Console.WriteLine(myReader[0].ToString() + myReader[1].ToString() + myReader[2].ToString() + " " + myReader[3].ToString());
                }

            }
            catch
            {
                wo_anterior = "-";
                wo_nueva = "-";
            }
            
            myCon.Close();
            #endregion


            int ultimobloqueo = 0;

            try
            {
                ultimobloqueo = db.t_boxingscan_cambiowo.OrderByDescending(x => x.folio).ToList().First().folio;
            }
            catch
            {
                ultimobloqueo = 0;
            }
            
            t_boxingscan_cambiowo newboxingscan_cambiowo = new t_boxingscan_cambiowo();

            newboxingscan_cambiowo.folio = ultimobloqueo + 1;
            newboxingscan_cambiowo.fecha = System.DateTime.Now;
            newboxingscan_cambiowo.bloqueo = 1;
            newboxingscan_cambiowo.linea = linea;
            newboxingscan_cambiowo.wo_anterior = wo_anterior.Replace(" ", String.Empty); 
            newboxingscan_cambiowo.wo_nueva = wo_nueva.Replace(" ", String.Empty); 

            db.t_boxingscan_cambiowo.Add(newboxingscan_cambiowo);
            db.SaveChanges();

        }

        public ActionResult Horaxhora(string linea)
        {
            ViewBag.linea = linea;
            var tlineainfo = db.t_lineconfd.Where(x => x.line == linea).First();
            string tlinea = tlineainfo.clave;
            string tlineadesc = tlineainfo.t_lineconf.descrip;
            string enmeta = "0";
            var tablaprogreso = db.sp_dailyline(linea, "1", System.DateTime.Now).OrderBy(x => x.consec).ToList();
            
            decimal metaactual = 50000;
            decimal cajasactual = 50000;
            decimal tcajas, tmeta;
            int tmuerto;
            var treshold = db.t_config.Find("686").threshold_boxscan;

            int consecactual = 50000;

            tcajas = tablaprogreso.Sum(x => x.cajas);
            tmeta = Convert.ToDecimal(tablaprogreso.Sum(x => x.meta));
            tmuerto = tablaprogreso.Sum(x => x.tmuerto);
            var tkits = tablaprogreso.Sum(x => x.kits);

            int hactual = System.DateTime.Now.Hour - 1;

            string rojo = "'#de5f5f'";
            string verde = "'#7dc975'";
            string azul = "'#5e9cf2'";
            string data1Graph = "[";
            string data2Graph = "[";
            string colorGraph = "[";
            string xAxisGraph = "[";
            int n = 0;
           

            foreach (var item in tablaprogreso)
            {
                if (hactual == Convert.ToInt16(item.hora.Substring(0, 2)))
                {
                    consecactual = Convert.ToInt16(item.consec);
                    tcajas = tablaprogreso.Where(x => x.consec <= item.consec).Sum(x => x.cajas);
                    tmuerto = tablaprogreso.Where(x => x.consec <= item.consec).Sum(x => x.tmuerto);
                    tkits = tablaprogreso.Where(x => x.consec <= item.consec).Sum(x => x.kits);
                    metaactual = Convert.ToDecimal(item.meta_ac);
                }

                n++;
                if(hactual < Convert.ToInt16(item.hora.Substring(0, 2)))
                {
                    data1Graph = data1Graph + "0";
                }
                else
                {
                    data1Graph = data1Graph + Convert.ToString(item.cajas);          
                }
                data2Graph = data2Graph + Convert.ToString(item.meta);
                xAxisGraph = xAxisGraph + "'" + Convert.ToString(item.hora.Substring(8,5)) + "'";

                if (item.cajas < item.meta)
                {
                    colorGraph = colorGraph + rojo;
                }
                else
                {
                    colorGraph = colorGraph + verde;
                }


                if (n == tablaprogreso.Count())
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

            if (metaactual == 50000)
            {
                metaactual = tmeta;
            }

            cajasactual = tcajas;

            if (metaactual <= cajasactual)
            {
                enmeta = "1";
            }

            var totalpromedio = 0.0;
            try
            {
                totalpromedio = Math.Round(Convert.ToDouble(cajasactual / metaactual) * 100);
            }
            catch
            {
                totalpromedio = 0;
            }

            int twos = 0;
            string fechactual = System.DateTime.Today.ToString("yyyy/MM/dd");

            var hola = db.t_boxingscan_d.Where(x => x.linea == linea && x.fecha ==fechactual).ToList();
            try
            {
              twos = Convert.ToInt32(db.t_boxingscan_d.Where(x => x.linea == linea && x.fecha == fechactual).First().wos);
            }
            catch
            {

            }
            tcajas =  Convert.ToDecimal(Math.Round(Convert.ToDouble(tcajas), 2));

            // var metah = db.t_lineconfh.Where(x => x.clave == tlinea).OrderBy(x=>x.consec).ToList();
            //var cajash = db.t_boxingscan_h.Where(x => x.linea == linea && DbFunctions.TruncateTime(x.fecha) >= DateTime.Today).OrderBy(x => x.hora).ToList();
            ViewBag.twos = twos;
            ViewBag.tcajas = tcajas;
            ViewBag.tmeta = metaactual;
            ViewBag.tkits = tkits;
            ViewBag.ttmuerto = tmuerto;
            ViewBag.tablaprogreso = tablaprogreso;
            ViewBag.enmeta = enmeta;
            ViewBag.consecactual = consecactual;
            ViewBag.treshold = treshold;
            ViewBag.tlineadesc = tlineadesc;
            ViewBag.promedio = totalpromedio;
            

            ViewBag.xAxisGraph = xAxisGraph;
            ViewBag.colorGraph = colorGraph;
            ViewBag.dataset1 = data1Graph;
            ViewBag.dataset2 = data2Graph;
            return View();
        }

        public ActionResult CleanRoom()
        {
            var infolineas = db.cleanroomview1(System.DateTime.Now).OrderBy(x => x.linea).ToList();
            var totalcajas = infolineas.Sum(x => x.cajas);
            var totalmeta = infolineas.Sum(x => x.meta_ac);
            var totalpromedio = 0.0;
            try
            {
                totalpromedio = Math.Round(Convert.ToDouble(totalcajas / totalmeta) * 100);
            }
            catch
            {
                totalpromedio = 0;
            }

            string color = "bg-success";

            if (totalpromedio < 85)
            {
                color = "bg-danger";
            }
            else
            if (totalpromedio < 95)
            {
                color = "bg-warning";
            }
            else
            {
                color = "bg-success";
            }

            totalcajas = Convert.ToDecimal(Math.Round(Convert.ToDouble(totalcajas),2));

            ViewBag.color = color;
            ViewBag.totalcajas = totalcajas;
            ViewBag.totalmeta = totalmeta;
            ViewBag.totalpromedio = totalpromedio;
            ViewBag.infolineas = infolineas;
            return View();
        }

        public ActionResult HistorialDiario()
        {
            ViewBag.listalineas = db.t_boxingscan_d.OrderByDescending(x=>x.fecha).ToList();
            return View();
        }

        public ActionResult BoxingLines()
        {
            return View(db.t_lineconfd.Include(t => t.t_lineconf).OrderBy(x => x.line).ToList());
        }

        public ActionResult ProdDiariaCR()
        {

            var linesInfo = db.cleanroomview1(System.DateTime.Now).OrderBy(x => x.linea).ToList();

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
                data1Graph = data1Graph + Convert.ToString(item.cajas);
                data2Graph = data2Graph + Convert.ToString(item.meta_ac);
                xAxisGraph = xAxisGraph + Convert.ToString(item.linea);

                if (item.cajas < item.meta_ac)
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
            var totalcajas = linesInfo.Sum(x => x.cajas);

           totalcajas= Convert.ToDecimal(Math.Round(Convert.ToDouble(totalcajas), 2));

            ViewBag.tcajas = totalcajas;
            ViewBag.tmeta = linesInfo.Sum(x => x.meta_ac);
            ViewBag.tkits = linesInfo.Sum(x => x.kits);
            ViewBag.ttmuerto = linesInfo.Sum(x => x.tmuerto);
            ViewBag.twos = linesInfo.Sum(x => x.wos);
            ViewBag.data1Graph = data1Graph;
            ViewBag.data2Graph = data2Graph;
            ViewBag.colorGraph = colorGraph;
            ViewBag.xAxisGraph = xAxisGraph;


            ViewBag.linesInfo = linesInfo;


            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
