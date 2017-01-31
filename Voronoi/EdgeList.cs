﻿using System;

namespace Voronoi
{
    internal class EdgeList
    {
        public HalfEdge LeftEnd { get; }
        public HalfEdge RightEnd { get; }
        private readonly int _hashSize;
        private readonly HalfEdge[] _hash;

        /// <summary>
        /// Initializes the array of HalfEdge's based on the number of sites available.
        /// </summary>
        /// <param name="sites">number of sites to create</param>
        internal EdgeList(int sites)
        {
            _hashSize = 2 * (int)Math.Sqrt(sites + 4);
            _hash = new HalfEdge[_hashSize];

            for (int i = 0; i < _hashSize; i++) _hash[i] = null;
            LeftEnd = new HalfEdge(null, 0);
            RightEnd = new HalfEdge(null, 0);
            LeftEnd.ElLeft = null;
            LeftEnd.ElRight = RightEnd;
            RightEnd.ElLeft = LeftEnd;
            RightEnd.ElRight = null;
            _hash[0] = LeftEnd;
            _hash[_hashSize - 1] = RightEnd;
        }

        /// <summary>
        /// Get entry from hash table, prunes any deleted nodes.
        /// </summary>
        /// <param name="b">entry location</param>
        /// <returns>half edge from table that corresponds to b</returns>
        private HalfEdge GetHash(int b)
        {
            if (b < 0 || b >= _hashSize)
                return null;
            var he = _hash[b];
            if (he?.ElEdge == null || he.ElEdge != null)
                return he;
            _hash[b] = null;
            return null;
        }

        /// <summary>
        /// Determines the HalfEdge nearest left to the given point.
        /// </summary>
        /// <param name="p">Point to find left bound HalfEdge from</param>
        /// <returns>left bound half edge of point</returns>
        internal HalfEdge LeftBound(Point2D p)
        {
            var bucket = 0;
            if (bucket >= _hashSize) bucket = _hashSize - 1;
            var he = GetHash(bucket);
            if (he == null)
            { 
                int i;
                for (i = 1; ; i++)
                {
                    if ((he = GetHash(bucket - i)) != null)
                        break;
                    if ((he = GetHash(bucket + i)) != null)
                        break;
                }
            }
            if (he == LeftEnd || (he != RightEnd && IsRightOf(he, p)))
            {
                do
                {
                    he = he.ElRight;
                } while (he != RightEnd && IsRightOf(he, p));

                he = he.ElLeft;
            }
            else
            {
                do
                {
                    he = he.ElLeft;
                } while (he != LeftEnd && !IsRightOf(he, p));
            }
            if (bucket > 0 && bucket < _hashSize - 1)
                _hash[bucket] = he;
            return he;
        }

        /// <summary>
        /// Deletes HalfEdge. This delete routine cannot reclaim the node, since
        /// hash table pointers may still be present.
        /// </summary>
        /// <param name="he">node to delete</param>
        internal void Delete(HalfEdge he)
        {
            he.ElLeft.ElRight = he.ElRight;
            he.ElRight.ElLeft = he.ElLeft;
            he.ElEdge = null;
        }

        /// <summary>
        /// Determines if the Point is to the right of the HalfEdge.
        /// </summary>
        /// <param name="el">edge</param>
        /// <param name="p">point</param>
        /// <returns>1 if p is to the right of HalfEdge</returns>
        private static bool IsRightOf(HalfEdge el, Point2D p)
        {
            bool above;
            var e = el.ElEdge;
            var topsite = e.Reg[1];
            var rightOfSite = p.X > topsite.X;
            if (rightOfSite && el.ElPm == 0)
                return true;
            if (!rightOfSite && el.ElPm == 1)
                return false;
            if (DblEql(e.A, 1.0))
            {
                var dyp = p.Y - topsite.Y;
                var dxp = p.X - topsite.X;
                var fast = false;
                if ((!rightOfSite && (e.B < 0.0)) || (rightOfSite && (e.B >= 0.0)))
                {
                    above = dyp >= e.B * dxp;
                    fast = above;
                }
                else
                {
                    above = p.X + p.Y * e.B > e.C;
                    if (e.B < 0.0)
                        above = !above;
                    if (!above)
                        fast = true;
                }
                if (fast) return el.ElPm == 0 ? above : !above;
                var dxs = topsite.X - e.Reg[0].X;
                above = e.B * (dxp * dxp - dyp * dyp) < dxs * dyp * (1.0 + 2.0 * dxp / dxs + e.B * e.B);
                if (e.B < 0.0)
                    above = !above;
            }
            else
            {
                var yl = e.C - e.A * p.X;
                var t1 = p.Y - yl;
                var t2 = p.X - topsite.X;
                var t3 = yl - topsite.Y;
                above = t1 * t1 > t2 * t2 + t3 * t3;
            }
            return el.ElPm == 0 ? above : !above;
        }

        /// <summary>
        /// Determines if two doubles are equal.
        /// </summary>
        /// <param name="a">first double</param>
        /// <param name="b">second double</param>
        /// <returns>true if the double are equal</returns>
        private static bool DblEql(double a, double b) => Math.Abs(a - b) < 0.00000000001;

        /// <summary>
        /// Inserts a new HalfEdge next to the previous one
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="newHe"></param>
        internal static void ElInsert(HalfEdge lb, HalfEdge newHe)
        {
            newHe.ElLeft = lb;
            newHe.ElRight = lb.ElRight;
            lb.ElRight.ElLeft = newHe;
            lb.ElRight = newHe;
        }
    }
}
