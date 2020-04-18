﻿using ChineseChess.Source.GameRule;
using ChineseChess.Source.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChess.Source.GameObjects.Chess
{
    public sealed class Horse : Piece
    {

        public Horse(Texture2D texture, Vector2 position, int type, ChessBoard board) : base(texture, position, type, board)
        {
        }


        protected override void FindLegalMoves(int[][] board)
        {
            base.FindLegalMoves(board);

            var IdxToVector2 = Index.ToVector2();
            FindLShapedMoves(IdxToVector2);
            RemoveIllegalMoves(board);
        }

        protected override void RemoveIllegalMoves(int[][] board)
        {
            LegalMoves.RemoveAll(OutOfRangeMove());

            LegalMoves.RemoveAll(c => board[c.Y][c.X] * Value > 0);

            LegalMoves.RemoveAll(c => IsBlockedMove(c, board));
        }

        protected override Predicate<Point> OutOfRangeMove()
        {
            return c => c.Y < 0 || c.Y >= (int)BoardRule.ROW ||
                        c.X < 0 || c.X >= (int)BoardRule.COL;
        }

        protected override bool IsBlockedMove(Point move, int[][] board)
        {
            if (Math.Abs(Index.X - move.X) == 2)
                return board[Index.Y][(Index.X + move.X) / 2] != 0;
            else
                return board[(Index.Y + move.Y) / 2][Index.X] != 0;
        }

        private void FindLShapedMoves(Vector2 currentPos)
        {
            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(2, 1)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(-2, 1)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(2, -1)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(-2, -1)).ToPoint());

            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(1, 2)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(-1, 2)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(1, -2)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPos, new Vector2(-1, -2)).ToPoint());
        }
    }
}
