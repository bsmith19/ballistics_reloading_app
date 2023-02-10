using System;
namespace gnu_ballistics
{
	public static class Atmosphere
	{
        /**
		* A function to correct a "standard" Drag Coefficient for differing atmospheric conditions.
		* @param drag_coefficient  G1, G2, G3, G4, G5, G6, G7, or G8
		* @param altitude          The altitude above sea level in feet.  Standard altitude is 0 feet above sea level.
		* @param barometer         The barometric pressure in inches of mercury (in Hg).
		*                          This is not "absolute" pressure, it is the "standardized" pressure reported in the papers and news.
		*                          Standard pressure is 29.53 in Hg.
		* @param temperature       The temperature in Fahrenheit.  Standard temperature is 59 degrees.
		* @param relative_humidity The relative humidity fraction.  Ranges from 0.00 to 1.00, with 0.50 being 50% relative humidity.
		*                          Standard humidity is 78%
		* @return The corrected drag coefficient for the supplied drag coefficient and atmospheric conditions.
		*/
        public static double atmosphere_correction(double drag_coefficient, double altitude, double barometer, double temperature,
                                     double relative_humidity)
		{
            double fa = calcFA(altitude);
            double ft = calcFT(temperature, altitude);
            double fr = calcFR(temperature, barometer, relative_humidity);
            double fp = calcFP(barometer);

            // Calculate the atmospheric correction factor
            double cd = (fa * (1 + ft - fp) * fr);
            return drag_coefficient * cd;
        }

        // Drag coefficient atmospheric corrections
        private static double calcFR(double temperature, double pressure, double relative_humidity)
        {
            double VPw = 4e-6 * Math.Pow(temperature, 3) - 0.0004 * Math.Pow(temperature, 2) + 0.0234 * temperature - 0.2517;
            double frh = 0.995 * (pressure / (pressure - (0.3783) * (relative_humidity) * VPw));
            return frh;
        }

        private static double calcFP(double pressure)
        {
            double p_std = 29.53; // in-hg; standard pressure at sea level
            double fp = 0;
            fp = (pressure - p_std) / (p_std);
            return fp;
        }

        private static double calcFT(double temperature, double altitude)
        {
            double t_std = -0.0036 * altitude + 59;
            double FT = (temperature - t_std) / (459.6 + t_std);
            return FT;
        }

        private static double calcFA(double altitude)
        {
            double fa = 0;
            fa = -4e-15 * Math.Pow(altitude, 3) + 4e-10 * Math.Pow(altitude, 2) - 3e-5 * altitude + 1;
            return (1 / fa);
        }
    }
}

