﻿using ChineseChess.Source.GameRule;
using ChineseChess.Source.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ChineseChess.Source.GameObjects.Chess
{
    public sealed class Advisor : Piece
    {
        public Advisor(Texture2D txt, Vector2 pos, int val, ChessBoard board) : base(txt, pos, val, board) { }


        protected override void FindLegalMoves(int[][] board)
        {
            base.FindLegalMoves(board);

            var IdxToVector2 = Index.ToVector2();
            FindCrossMove(IdxToVector2);

            RemoveIllegalMoves(board);
        }

        private void FindCrossMove(Vector2 currentPosition)
        {
            LegalMoves.Add(Vector2.Add(currentPosition, new Vector2(1, 1)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPosition, new Vector2(1, -1)).ToPoint());

            LegalMoves.Add(Vector2.Add(currentPosition, new Vector2(-1, 1)).ToPoint());
            LegalMoves.Add(Vector2.Add(currentPosition, new Vector2(-1, -1)).ToPoint());
        }

        protected override void RemoveIllegalMoves(int[][] board)
        {
            LegalMoves.RemoveAll(OutOfRangeMove());

            LegalMoves.RemoveAll(c => board[c.Y][c.X] * Value > 0);
        }

        protected override Predicate<Point> OutOfRangeMove()
        {
            return c => c.Y < 0 || c.Y >= (int)BoardRule.ROW ||
                        c.X > (int)BoardRule.R_CASTLE || 
                        c.X < (int)BoardRule.L_CASTLE ||
                        Value > 0 && c.Y < (int)BoardRule.FR_CASTLE ||
                        Value < 0 && c.Y > (int)BoardRule.FB_CASTLE;
        }
    }
}
