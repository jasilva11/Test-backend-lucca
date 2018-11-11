using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ShortPath
{
    public class Route
    {
        public string from { get; set; }

        public string to { get; set; }

        public decimal convertion { get; set; }

        public List<Route> linkedRoutes { get; set; }

        public List<Route> chainedRoutes { get; set; }

        public Route()
        {
            this.linkedRoutes = new List<Route>();
            this.chainedRoutes = new List<Route>();
        }
        
        public void IndexLinkedRoutes(List<Route> routes)
        {
            this.linkedRoutes.AddRange(routes.Where(route => route.from == this.from && route.to != this.to));
            this.chainedRoutes.AddRange(routes.Where(route => route.from == this.to && route.to != this.from));
        }

        public List<Route> GetRoutes()
        {
            var routes = new List<Route>();

            routes.AddRange(this.linkedRoutes);

            return routes;
        }
        
        public List<Route> GetChainedRoutes()
        {
            var routes = new List<Route>();

            foreach (var route in this.chainedRoutes)
            {
                var complexRoute = new Route()
                {
                    from = string.Concat(this.from, ", ", route.from),
                    to = route.to,
                    chainedRoutes = route.chainedRoutes,
                    linkedRoutes = route.linkedRoutes,
                    convertion = Math.Round(this.convertion * route.convertion, 4)
                };

                routes.Add(complexRoute);

                foreach (var chain in route.GetChainedRoutes())
                {
                    var complexChain = new Route()
                    {
                        from = string.Concat(this.from, ", ", chain.from),
                        to = chain.to,
                        chainedRoutes = chain.chainedRoutes,
                        linkedRoutes = chain.linkedRoutes,
                        convertion = Math.Round(this.convertion * chain.convertion, 4)
                    };

                    routes.Add(complexChain);
                }
            }

            return routes;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Vous devez passer la route d'un fichier txt en paramettres");
            }
            else
            {
                if (args.Length == 1)
                {
                    string[] lines = System.IO.File.ReadAllLines(args[0]);
                    List<string> csvLines = new List<string>();
                    for (int i = 2; i < 2 + int.Parse(lines[1]); i++)
                    {
                        csvLines.Add(lines[i]);
                    }

                    var routes = new List<Route>();
                    csvLines.ForEach(s =>
                    {
                        string[] pieces = s.Split(';');

                        decimal newConvertion = decimal.Parse(pieces[2], CultureInfo.InvariantCulture);

                        var route = new Route()
                        {
                            from = pieces[0],
                            to = pieces[1],
                            convertion = newConvertion
                        };

                        var route2 = new Route()
                        {
                            from = pieces[1],
                            to = pieces[0],
                            convertion = Math.Round(1 / newConvertion, 4)
                        };

                        routes.Add(route);
                        routes.Add(route2);
                    });

                    routes.ForEach(route =>
                    {
                        route.IndexLinkedRoutes(routes);
                        var str = route.from + "," + route.to;
                    });

                    string selectedFrom = lines[0].Split(';')[0];
                    var amount = int.Parse(lines[0].Split(';')[1]);
                    string selectedTo = lines[0].Split(';')[2].Replace(" ", "");

                    bool directRoutesOnly = false;

                    var desiredRoutes = new List<Route>();

                    if (directRoutesOnly)
                    {
                        desiredRoutes = routes.Where(route => route.from == selectedFrom).OrderBy(route => route.convertion).ToList();
                    }
                    else
                    {
                        routes.Where(route => route.from == selectedFrom).ToList().ForEach(route =>
                        {
                            desiredRoutes.AddRange(route.GetRoutes());
                            desiredRoutes.AddRange(route.GetChainedRoutes());
                        });

                        routes = routes.OrderBy(x => x.convertion).ToList();
                    }

                    var finals = desiredRoutes.Where(route => route.to == selectedTo).OrderBy(route => route.convertion).ToList();
                    var res = int.MaxValue;

                    foreach (var elem in finals)
                    {
                        if (Math.Round(amount * elem.convertion, 0) < res)
                        {
                            res = (int) Math.Round(amount * elem.convertion, 0);
                        }
                    }

                    Console.WriteLine(res);
                }
                else
                {
                    Console.WriteLine("Vous devez passer qu'un seul paramettre");
                }
            }
            Console.WriteLine("Appuyez sur une touche pour quitter.");
            Console.ReadKey();
        }
    }
}
