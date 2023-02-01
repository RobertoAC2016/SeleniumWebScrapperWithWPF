using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace WebScrappingWPF
{
    public partial class MainWindow : Window
    {
        private ChromeDriver dr;
        private String url = "https://www.freeproxylists.net/";
        private ChromeDriverService cds = ChromeDriverService.CreateDefaultService();
        private ChromeOptions ops = new ChromeOptions();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cds.HideCommandPromptWindow = true;
            ops.AddArgument("headless");          //al comentar esta linea el browser se abrira
            dr = new ChromeDriver(cds, ops);
            dr.Navigate().GoToUrl(url);
        }
        private void btnrunscrapper_Click(object sender, RoutedEventArgs e)
        {
            //  Manejaremos el procesado de la informacion con un thread de modo q la aplicacion no sea bloqueada
            lblstatus.Content = "Running...";
            //vamos a manejar el proceso con thread para evitar q sea bloqueada la aplicacion proncipal
            Thread hilo = new Thread(() => {
                get_rows();
            });
            hilo.Start();
        }
        private void get_rows()
        {
            //el maximo numero de paginas de la pagina de proxis es de 15 por lo q
            //necesitamos hacer el ciclo de paginacion 15 veces
            var pages = 15;
            Dispatcher.Invoke(() => {
                lblstatus.Content = $"Running: {DateTime.Now}, Pagina: 1";
            });
            for (int i = 0; i < pages; i++)
            {
                bool header = true; //con esta variable determinaremos q al recorrer la tabla de proxis nos saltemos el titulo
                                    //              /html/body/div/div[2]/table/tbody/tr    con este path extraido de la pagina obtenemos las filas
                                    //  Para invokar un elemento que esta fuera del thread principal en este caso el scrapper,
                                    //  usaremos un delegado, actualizaremos el label con la fecha y la pagina q se esta consultando
                
                var rows = dr.FindElements(By.XPath("/html/body/div/div[2]/table/tbody/tr"));
                foreach (var row in rows)
                {
                    //aqui saltamos la  primera fila q contiene los titulos
                    if (header)
                    {
                        header = false;
                        continue;
                    }
                    //  En esta variable obtenemos cada una de las celdas con los datos
                    var cells = row.FindElements(By.TagName("td"));
                    //la celda 0 trae la IP, la celda 1 trae el puerto y la celda 7 trae el porcentaje de exitos de conexion
                    if (cells.Count >= 10)
                    {
                        var line = $"fila => {cells[0].Text}, {cells[1].Text}, {cells[7].Text}";
                        Console.WriteLine(line);
                        Debug.WriteLine(line);
                    }
                }
                // ya que la invocacion del evento click no funciono, usaremos la nevegacion
                dr.Navigate().GoToUrl($"https://www.freeproxylists.net/?page={i + 1}");
                Thread.Sleep(1000);

                Dispatcher.Invoke(() => {
                    lblstatus.Content = $"Running: {DateTime.Now}, Pagina: {i + 1}";
                });
                //  Ahora al terminar de recorrer los filas de la pagina pasamos a la siguiente pagina
                //var btnnextpage = dr.FindElement(By.XPath("/html/body/div/div[2]/div[2]/a"));
                //  Como es un hiperlink podemos usar el evento click
                //btnnextpage.Click();
                //esperamos un segundo para continuar
                //      /html/body/div/div[2]/div[2]/a[14]
            }
        }
    }
}
