﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChess.Source.GameRule
{
    enum GameState
    {
        B_WIN,
        R_WIN,
        CHECKMATE,
        B_TURN,
        R_TURN,
        MOVING,
        GAMEOVER,
        IDLE
    }
}
