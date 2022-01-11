using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toc_Core.Game
{
    public enum Game_Field 
    {
        X = 0,
        O = 1,
        NULL = 2
    }
    public class Game
    {
        public bool Is_End_Game { get; set; } = false;
        private List<Game_Field> Game_Fields = new List<Game_Field>();
        public Game_Field Flag_Step { get; private set; } = Game_Field.X;
        public IReadOnlyList<Game_Field> Fields => Game_Fields;
        public Game()
        {
            for (int i = 0; i < 9; i++)
            {
                Game_Fields.Add(Game_Field.NULL);
            }
        }
        public void Restart()
        {
            for (int i = 0; i < 9; i++)
            {
                Game_Fields[i] = Game_Field.NULL;
            }
            Flag_Step = Game_Field.X;
        }

        public bool Set_Cell(int position, Game_Field Cell)
        {
            if (Game_Fields[position] == Game_Field.NULL)
            {
                Game_Fields[position] = Cell;
                if (Flag_Step == Game_Field.X)
                    Flag_Step = Game_Field.O;
                else
                    Flag_Step = Game_Field.X;
                return true;
            }
            return false;
        }

        public bool Check_Win(out Game_Field Cell)
        {
            // Rows
            for (int i = 0; i < 3; i++)
            {
                if (Game_Fields[i] == Game_Fields[i+1] && Game_Fields[i+1] == Game_Fields[i+2] && Game_Fields[i] != Game_Field.NULL)
                {
                    Cell = Game_Fields[i];
                    return true;
                }
            }
            // Columns
            for (int i = 0; i < 3; i++)
            {
                if (Game_Fields[i] == Game_Fields[i+3] && Game_Fields[i+3] == Game_Fields[i+6] && Game_Fields[i] != Game_Field.NULL)
                {
                    Cell = Game_Fields[i];
                    return true;
                }
            }
            // Diagonals
            if (Game_Fields[0] == Game_Fields[4] && Game_Fields[4] == Game_Fields[8] && Game_Fields[4] != Game_Field.NULL)
            {
                Cell = Game_Fields[0];
                return true;
            }
            if (Game_Fields[2] == Game_Fields[4] && Game_Fields[4] == Game_Fields[6] && Game_Fields[4] != Game_Field.NULL)
            {
                Cell = Game_Fields[2];
                return true;
            }
            Cell = Game_Field.NULL;
            return false;
        }

        public bool Check_Draw()
        {
            foreach (Game_Field item in Game_Fields)
            {
                if(item == Game_Field.NULL)
                    return false;
            }
            return true;
        }
    }
}
