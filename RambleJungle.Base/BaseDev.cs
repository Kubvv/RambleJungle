﻿namespace RambleJungle.Base
{
    public class BaseDev
    {
        private readonly int baseValue = 0;
        private readonly int deviation = 0;

        public BaseDev(int baseValue, int deviation)
        {
            this.baseValue = baseValue;
            this.deviation = deviation;
        }

        public int RandomValue => baseValue + Config.Random.Next(deviation * 2 + 1) - deviation;
    }
}
