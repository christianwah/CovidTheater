using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    [ApiController]
    public class PersonController : ControllerBase
    {
        private static int min_distance = 7;
        private static int currentRow = 100;
        private static int currentColumn = 100;
        private static int[,] seating = new int[currentRow, currentColumn];
        private static List<Point> occupiedList = new List<Point>();

        private readonly ILogger<PersonController> _logger;

        public PersonController(ILogger<PersonController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public ActionResult<string> Get(int? row, int? column, int? minimumDistance)
        {
            if (row != null)
                currentRow = row.Value;
            if (column != null)
                currentColumn = column.Value;
            if (minimumDistance != null)
                min_distance = minimumDistance.Value;
            if (row != null || column != null)
            {
                seating = new int[currentRow, currentColumn];
                MarkUnavailable();
                return "seating modified";
            }
            else
                return "no change";
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public ActionResult<string> Clear()
        {
            seating = new int[currentRow, currentColumn];
            occupiedList.Clear();
            return "cleared";
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<ActionResult<string>> Reserve(int row, int column)
        {
            if (row >= currentRow && column >= currentColumn)
                return "invalid row/column";
            lock (occupiedList)
            {
                if (occupiedList.Any(e => !MinDistance(row, column, e)))
                    return "not minimal distance";

                occupiedList.Add(new Point(row, column));
            }
            MarkUnavailable(new Point(row, column));
            return "seat is reserved";
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public ActionResult<string> Book(int noOfPeople)
        {
            // find empty coordinate
            Point? empty = null;
            for (int i = 0; i < currentRow; i++)
            {
                for (int j = 0; j < currentRow; j++)
                {
                    if (seating[i, j] == 0)
                    {
                        empty = new Point { X = i, Y = j };
                    }
                }
            }
            // check if enough for noOfPeople
            if (empty.HasValue)
            {
                List<string> pos = new List<string>();
                int seatedGroup = 0;
                void work()
                {
                    for (int i = 0; i < noOfPeople; i++)
                    {
                        for (int j = 0; j < noOfPeople; j++)
                        {
                            if (seating[empty.Value.X + i, empty.Value.Y + j] == 0)
                            {
                                seatedGroup++;
                            }
                            if (seatedGroup == noOfPeople)
                            {
                                return;
                            }
                        }
                    }
                }
                work();
                return "booked on " + string.Join(',', pos);
            }
            else
            {
                return "no change";
            }
        }

        private bool MinDistance(int x, int y, Point person)
        {
            int dist = Math.Abs(person.X - x) + Math.Abs(person.Y - y);
            if (dist >= min_distance)
                return true;
            else
                return false;
        }

        private async void MarkUnavailable()
        {
            foreach (Point point in occupiedList)
            {
                for (int i = point.X - min_distance; i < point.X + min_distance; i++)
                {
                    for (int j = point.Y - min_distance; j < point.Y + min_distance; j++)
                    {
                        if (MinDistance(i, j, point))
                            seating[i, j] = 1;
                    }
                }
            }
        }

        private async void MarkUnavailable(Point point)
        {
            for (int i = point.X - min_distance; i < point.X + min_distance; i++)
            {
                for (int j = point.Y - min_distance; j < point.Y + min_distance; j++)
                {
                    if (MinDistance(i, j, point))
                        seating[i, j] = 1;
                }
            }
        }
    }
}
