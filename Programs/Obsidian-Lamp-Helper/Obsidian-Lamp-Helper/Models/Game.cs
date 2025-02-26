using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian_Lamp_Helper.Models
{
    public class Game
    {
        public List<Pack> Packs { get; set; }

        public Game()
        {
            Packs = new List<Pack>()
            {
                new Pack(
                    "pack 1", 
                    [
                    new Card("card 1"),
                    new Card("card 2"),
                    new Card("card 3"),
                    new Card("card 4")
                    ]),
                new Pack(
                    "pack 2",
                    [
                    new Card("card 1"),
                    new Card("card 2"),
                    new Card("card 3"),
                    new Card("card 4")
                    ])
            };

            var jsonFileManager = new JsonFileManager();
            _ = jsonFileManager.SaveToFileAsync("Decks.json", Packs);
        }
    }
}
