﻿using ChineseChess.Source.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChineseChess.Source.GameRule;

namespace ChineseChess.Source.GameObjects.Chess
{
    public sealed class Cannon : Piece
    {
        public Cannon(Texture2D texture, Vector2 position, int type, ChessBoard board) : base(texture, position, type, board) { }


        protected override void FindLegalMoves(int[][] board)
        {
            base.FindLegalMoves(board);
            FindHorizontalMoves(board);
            FindVerticalMoves(board);
        }

        protected override void FindHorizontalMoves(int[][] board)
        {
            int posY = Index.Y;
            for (int i = Index.X + 1; i < (int)BoardRule.COL; ++i)
            {
                if (board[posY][i] != 0)
                {
                    while (i < (int)BoardRule.COL - 1)
                    {
                        ++i;
                        if (board[posY][i] * Value > 0) break;
                        if (board[posY][i] * Value < 0)
                            LegalMoves.Add(new Point(i, posY));
                            break;
                    }
                    break;
                }
                else
                    LegalMoves.Add(new Point(i, posY));
            }

            if (Index.X - 1 < 0) return;
            for (int i = Index.X - 1; i >= 0; --i)
            {
                if (board[posY][i] != 0)
                {
                    while (i > 0)
                    {
                        --i;
                        if (board[posY][i] * Value > 0) break;
                        if (board[posY][i] * Value < 0)
                            LegalMoves.Add(new Point(i, posY));
                            break;
                    }
                    break;
                }
                else
                    LegalMoves.Add(new Point(i, posY));
            }
        }

        protected override void FindVerticalMoves(int[][] board)
        {
            int posX = Index.X;
            for (int i = Index.Y + 1; i < (int)BoardRule.ROW; ++i)
            {
                if (board[i][posX] != 0)
                {
                    while (i < (int)BoardRule.ROW - 1)
                    {
                        ++i;
                        if (board[i][posX] * Value > 0) break;
                        if (board[i][posX] * Value < 0)
                            LegalMoves.Add(new Point(posX, i));
                            break;
                    }
                    break;
                }
                else
                    LegalMoves.Add(new Point(posX, i));
            }

            if (Index.Y - 1 < 0) return;
            for (int i = Index.Y - 1; i >= 0; --i)
            {
                if (board[i][posX] != 0)
                {
                    while (i > 0)
                    {
                        --i;
                        if (board[i][posX] * Value > 0) break;
                        if (board[i][posX] * Value < 0)
                            LegalMoves.Add(new Point(posX, i));
                            break;
                    }
                    break;
                }
                else
                    LegalMoves.Add(new Point(posX, i));
            }
        }
    }
}
