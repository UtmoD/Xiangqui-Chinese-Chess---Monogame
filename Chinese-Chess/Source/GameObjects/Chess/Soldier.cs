﻿using ChineseChess.Source.GameRule;
using ChineseChess.Source.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChess.Source.GameObjects.Chess
{
    public sealed class Soldier : Piece
    {
        public Soldier(Texture2D texture, Vector2 position, int type, ChessBoard board) : base(texture, position, type, board)
        {
        }


        protected override void FindLegalMoves(int[][] board)
        {
            base.FindLegalMoves(board);
            FindVerticalMoves(board);
            if (RiverCrossed())
            {
                FindHorizontalMoves(board);
            }
        }

        private bool RiverCrossed()
        {
            return Value < 0 && Index.Y > (int)BoardRule.B_BORD ||
                   Value > 0 && Index.Y < (int)BoardRule.R_BORD;
        }

        protected override void FindHorizontalMoves(int[][] board)
        {
            if (Index.X + 1 < (int)BoardRule.COL)
            {
                StillHasLegalMoves(Index.Y, Index.X + 1, board);
            }
            if (Index.X - 1 >= 0)
            {
                StillHasLegalMoves(Index.Y, Index.X - 1, board);
            }
        }

        protected override void FindVerticalMoves(int[][] board)
        {
            var step = 1;
            if (Value > 0)
            {
                step = -step;
            }
            StillHasLegalMoves(Index.Y + step, Index.X, board);
        }
    }
}
