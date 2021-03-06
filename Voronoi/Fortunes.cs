﻿using System;
using System.Collections.Generic;

namespace Voronoi
{
    /**
     * Holy legacy code batman!
     * C @author Steven Fortune
     * C++ @author Shane O'Sullivan
     * Java @author Ryan Minor
     * C# @author Ryan Minor
     * 
     * Structs carried over from C++>Java code: 
     * 	Freenode. Knows next node. Implemented
     * 	FreeNodeArrayList. Knows current and next node. Implemented
     * 	FreeList. Knows head and size. Replaced with ArrayList
     * 	Point. x,y. Using Java's 2D > In C#, had to make own. Basically a quicker copy of it
     * 	Site. Stores a coord and 2 points. Implemented, then replaced with Point
     * 	Edge. Stores 3 doubles, 2 arrays of size 2 of sites, and one int. Implemented
     * 	Graphedge. Stores 2 x/y coords and a next graphedge.
     */

    public class Fortunes
    {
        private GraphEdge _allEdges;
        private Point2D _bottomsite;
        private int _siteidx;
        private Point2D[] _sites;
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        public int NumSites { get; private set; }

        /**
         * Creates object
         * Runs
         * Outputs
         */

        public static VoronoiOutput Run(int width, int height, int siteCount)
        {
            var fortune = new Fortunes
                          {
                              ImageWidth = width,
                              ImageHeight = height
                          };

            var values = GetSet(siteCount, width, height);

            //Run
            fortune.GenerateVoronoi(values);

            return new VoronoiOutput(fortune._allEdges, values);
        }

        /**
         * Creates object
         * Runs
         * Outputs
         * Uses manual set (for testing, mainly)
         */

        public static VoronoiOutput Run(int width, int height, Point2D[] values)
        {
            var fortune = new Fortunes
                          {
                              ImageWidth = width,
                              ImageHeight = height
                          };

            //Run
            fortune.GenerateVoronoi(values);

            return new VoronoiOutput(fortune._allEdges, values);
        }

        public bool GenerateVoronoi(Point2D[] values)
        {
            NumSites = values.Length;
            _sites = new Point2D[NumSites];

            if (values.Length == 0)
                return false;

            var xmin = values[0].X;
            var ymin = values[0].Y;
            var xmax = values[0].X;
            var ymax = values[0].Y;

            for (var i = 0; i < NumSites; i++)
            {
                _sites[i] = values[i];

                if (values[i].X < xmin)
                    xmin = values[i].X;
                else if (values[i].X > xmax)
                    xmax = values[i].X;

                if (values[i].Y < ymin)
                    ymin = values[i].Y;
                else if (values[i].Y > ymax)
                    ymax = values[i].Y;
            }

            // Sort x
            for (var n = 1; n < _sites.Length; n++)
            {
                var tem = _sites[n];
                int j;
                for (j = n - 1; (j >= 0) && (tem.X < _sites[j].X); j--)
                    _sites[j + 1] = _sites[j];

                _sites[j + 1] = tem;
            }

            // Sort y
            for (var n = 1; n < _sites.Length; n++)
            {
                var tem = _sites[n];
                int j;
                for (j = n - 1; (j >= 0) && (tem.Y < _sites[j].Y); j--)
                    _sites[j + 1] = _sites[j];

                _sites[j + 1] = tem;
            }

            _siteidx = 0;

            Voronoi();

            return true;
        }

        private Point2D Leftreg(HalfEdge he)
        {
            if (he.Edge == null)
                return _bottomsite;

            return he.Midpoint == 0 ? he.Edge.Reg[0] : he.Edge.Reg[1];
        }

        private Point2D Rightreg(HalfEdge he)
        {
            if (he.Edge == null) //if this HalfEdge has no edge, return the bottom site (whatever that is)
                return _bottomsite;

            //if the elPm field is zero, return the site 0 that this edge bisects, otherwise return site number 1
            return he.Midpoint == 0 ? he.Edge.Reg[1] : he.Edge.Reg[0];
        }

        private static Edge Bisect(Point2D s1, Point2D s2)
        {
            var newedge = new Edge();

            newedge.Reg[0] = s1; //store the sites that this edge is bisecting
            newedge.Reg[1] = s2;
            newedge.EndPoints[0] = null; //to begin with, there are no endpoints on the bisector - it goes to infinity
            newedge.EndPoints[1] = null;

            var dx = s2.X - s1.X;
            var dy = s2.Y - s1.Y;
            var adx = dx > 0 ? dx : -dx;
            var ady = dy > 0 ? dy : -dy;
            newedge.C = s1.X*dx + s1.Y*dy + (dx*dx + dy*dy)*0.5; //get the slope of the line

            if (adx > ady)
            {
                newedge.A = 1.0;
                newedge.B = dy/dx;
                newedge.C /= dx; //set formula of line, with x fixed to 1
            }
            else
            {
                newedge.B = 1.0;
                newedge.A = dx/dy;
                newedge.C /= dy; //set formula of line, with y fixed to 1
            }

            return newedge;
        }

        /// <summary>
        ///     Creates a new site where the HalfEdges intersect.
        /// </summary>
        /// <param name="el1">HalfEdge to intersect</param>
        /// <param name="el2">HalfEdge to intersect</param>
        /// <returns>new site/point at intersection</returns>
        private static Point2D Intersect(HalfEdge el1, HalfEdge el2)
        {
            Edge e;
            HalfEdge el;

            var e1 = el1.Edge;
            var e2 = el2.Edge;
            if ((e1 == null) || (e2 == null) || (e1.Reg[1] == e2.Reg[1]))
                return null;

            var d = e1.A*e2.B - e1.B*e2.A;
            //This checks for the value being basically zero
            if ((-0.0000000001 < d) && (d < 0.0000000001))
                return null;

            var xint = (e1.C*e2.B - e2.C*e1.B)/d;
            var yint = (e2.C*e1.A - e1.C*e2.A)/d;

            if ((e1.Reg[1].Y < e2.Reg[1].Y) || (DoubleComparison.IsEqual(e1.Reg[1].Y, e2.Reg[1].Y) &&
                                                (e1.Reg[1].X < e2.Reg[1].X)))
            {
                el = el1;
                e = e1;
            }
            else
            {
                el = el2;
                e = e2;
            }

            return ((xint >= e.Reg[1].X) && (el.Midpoint == 0)) || ((xint < e.Reg[1].X) && (el.Midpoint == 1)) ? null : new Point2D(xint, yint);
        }

        private void Endpoint(Edge e, int lr, Point2D s)
        {
            e.EndPoints[lr] = s;
            if (e.EndPoints[1 - lr] != null) ClipLine(e);
        }

        private void PushGraphEdge(double x1, double y1, double x2, double y2)
        {
            var newEdge = new GraphEdge(new Point2D(x1, y1), new Point2D(x2, y2), _allEdges);
            _allEdges = newEdge;
        }

        private void ClipLine(Edge e)
        {
            double x1;
            double x2;
            double y1;
            double y2;

            //if the distance between the two points this line was created from is less than 
            //the square root of 2, then ignore it
            if (e.Reg[0].Distance(e.Reg[1]) < 1.41421356) return;

            var s1 = DoubleComparison.IsEqual(e.A, 1.0) && (e.B >= 0.0) ? e.EndPoints[1] : e.EndPoints[0];
            var s2 = DoubleComparison.IsEqual(e.A, 1.0) && (e.B >= 0.0) ? e.EndPoints[0] : e.EndPoints[1];

            if (DoubleComparison.IsEqual(e.A, 1.0))
            {
                y1 = (s1 != null) && (s1.Y > 1) ? s1.Y : 1;
                y1 = y1 > ImageHeight ? ImageHeight : y1;
                x1 = e.C - e.B*y1;
                y2 = (s2 != null) && (s2.Y < ImageHeight) ? s2.Y : ImageHeight;
                y2 = y2 < 1 ? 1 : y2;
                x2 = e.C - e.B*y2;

                if (((x1 > ImageWidth) && (x2 > ImageWidth)) || ((x1 < 1) && (x2 < 1))) return;

                x1 = x1 > ImageWidth ? ImageWidth : x1;
                x1 = x1 < 1 ? 1 : x1;
                y1 = (x1 > ImageWidth) || (x1 < 1) ? (e.C - x1)/e.B : y1;
                x2 = x2 > ImageWidth ? ImageWidth : x2;
                x2 = x2 < 1 ? 1 : x2;
                y2 = (x2 > ImageWidth) || (x2 < 1) ? (e.C - x2)/e.B : y2;
            }
            else
            {
                x1 = (s1 != null) && (s1.X > 1) ? s1.X : 1;
                x1 = x1 > ImageWidth ? ImageWidth : x1;
                y1 = e.C - e.A*x1;
                x2 = (s2 != null) && (s2.X < ImageWidth) ? s2.X : ImageWidth;
                x2 = x2 < 1 ? 1 : x2;
                y2 = e.C - e.A*x2;

                if (((y1 > ImageHeight) & (y2 > ImageHeight)) | ((y1 < 1) & (y2 < 1))) return;

                y1 = y1 > ImageHeight ? ImageHeight : y1;
                y1 = y1 < 1 ? 1 : y1;
                x1 = (y1 > ImageHeight) || (y1 < 1) ? (e.C - y1)/e.A : x1;
                y2 = y2 > ImageHeight ? ImageHeight : y2;
                y2 = y2 < 1 ? 1 : y2;
                x2 = (y2 > ImageHeight) || (y2 < 1) ? (e.C - y2)/e.A : x2;
            }

            PushGraphEdge(x1, y1, x2, y2);
        }

        /// <summary>
        ///     Starts fortune's algorithm.
        /// </summary>
        private void Voronoi()
        {
            Point2D newintstar = null;
            HalfEdge leftBound;

            var queue = new PriorityQueue(NumSites);
            _bottomsite = NextSite();
            var list = new EdgeList(NumSites);
            var newsite = NextSite();

            while (true)
            {
                Point2D bot;
                Point2D p;
                HalfEdge rbnd;
                HalfEdge bisector;
                Edge e;

                if (!queue.IsEmpty())
                    newintstar = queue.Min();

                //if the lowest site has a smaller y value than the lowest vector intersection, process the site
                //otherwise process the vector intersection		
                if ((newsite != null) && (queue.IsEmpty() || (newintstar == null) || (newsite.Y < newintstar.Y) || (DoubleComparison.IsEqual(newsite.Y, newintstar.Y) && (newsite.X < newintstar.X))))
                { /* new site is smallest - this is a site event*/
                    leftBound = list.LeftBound(newsite); //get the first HalfEdge to the LEFT of the new site
                    rbnd = leftBound.Right; //get the first HalfEdge to the RIGHT of the new site
                    bot = Rightreg(leftBound); //if this HalfEdge has no edge, , bot = bottom site (whatever that is)
                    e = Bisect(bot, newsite); //create a new edge that bisects 
                    bisector = new HalfEdge(e, 0); //create a new HalfEdge, setting its elPm field to 0			
                    EdgeList.ElInsert(leftBound, bisector); //insert this new bisector edge between the left and right vectors in a linked list	

                    if ((p = Intersect(leftBound, bisector)) != null) //if the new bisector intersects with the left edge, remove the left edge's vertex, and put in the new one
                        queue.PQinsert(queue.Delete(leftBound), p, p.Distance(newsite));

                    leftBound = bisector;
                    bisector = new HalfEdge(e, 1); //create a new HalfEdge, setting its elPm field to 1
                    EdgeList.ElInsert(leftBound, bisector); //insert the new HE to the right of the original bisector earlier in the IF stmt

                    if ((p = Intersect(bisector, rbnd)) != null) //if this new bisector intersects with the
                        queue.PQinsert(bisector, p, p.Distance(newsite)); //push the HE into the ordered linked list of vertices

                    newsite = NextSite();
                }
                else if (!queue.IsEmpty())
                { /* intersection is smallest - this is a vector event */
                    leftBound = queue.ExtractMin(); //pop the HalfEdge with the lowest vector off the ordered list of vectors				
                    var llbnd = leftBound.Left;
                    rbnd = leftBound.Right; //get the HalfEdge to the right of the above HE
                    var rrbnd = rbnd.Right;
                    bot = Leftreg(leftBound); //get the Site to the left of the left HE which it bisects
                    var top = Rightreg(rbnd);

                    var leftBoundVertex = leftBound.Vertex;
                    Endpoint(leftBound.Edge, leftBound.Midpoint, leftBoundVertex); //set the endpoint of the left HalfEdge to be this vector
                    Endpoint(rbnd.Edge, rbnd.Midpoint, leftBoundVertex); //set the endpoint of the right HalfEdge to be this vector
                    list.Delete(leftBound); //mark the lowest HE for deletion - can't delete yet because there might be pointers to it in Hash Map	
                    queue.Delete(rbnd); //remove all vertex events to do with the  right HE
                    list.Delete(rbnd); //mark the right HE for deletion - can't delete yet because there might be pointers to it in Hash Map	
                    var pm = 0;

                    if (bot.Y > top.Y)
                    { //if the site to the left of the event is higher than the Site to the right of it, then swap them and set the 'pm' variable to 1
                        var temp = bot;
                        bot = top;
                        top = temp;
                        pm = 1;
                    }

                    e = Bisect(bot, top); //Create Edge between the two Sites
                    bisector = new HalfEdge(e, pm); //Create a HE from the Edge 'e', and make it point to that edge with its elEdge field
                    EdgeList.ElInsert(llbnd, bisector); //Insert the new bisector to the right of the left HE
                    Endpoint(e, 1 - pm, leftBoundVertex); //Set one endpoint to the new edge to be the vector point 'v'

                    //If left HE and the new bisector don't intersect, then delete the left HE, and reinsert
                    if ((p = Intersect(llbnd, bisector)) != null)
                        queue.PQinsert(queue.Delete(llbnd), p, p.Distance(bot));

                    //If right HE and the new bisector don't intersect, then reinsert it
                    if ((p = Intersect(bisector, rrbnd)) != null)
                        queue.PQinsert(bisector, p, p.Distance(bot));
                }
                else break;
            }

            for (leftBound = list.LeftEnd.Right; leftBound != list.RightEnd; leftBound = leftBound.Right)
                ClipLine(leftBound.Edge);
        }

        /// <summary>
        ///     Gets the next site from sites already in storage.
        /// </summary>
        /// <returns>single in-storage site</returns>
        private Point2D NextSite() => _siteidx >= NumSites ? null : _sites[_siteidx++];

        /// <summary>
        ///     Returns a set of random-ish points
        ///     Points are all multiples of 5,
        ///     this ensures points never touch/intersect lines
        /// </summary>
        /// <param name="size"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns> Array of unique Point2D's</returns>
        private static Point2D[] GetSet(int size, int x, int y)
        {
            var unique = new HashSet<string>();
            var values = new Point2D[size];
            var rand = new Random();

            while (unique.Count < size)
            {
                var point = new Point2D(rand.Next(5, x)/5*5, rand.Next(5, y)/5*5);
                if (unique.Add(point.ToString())) //If string is unique, point must be unique
                    values[unique.Count - 1] = point;
            }

            return values;
        }
    }
}