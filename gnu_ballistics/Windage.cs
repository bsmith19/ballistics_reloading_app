using System;
namespace gnu_ballistics
{
	public static class Windage
	{

        /**
        * A function to compute the windage deflection for a given crosswind speed,
        * given flight time in a vacuum, and given flight time in real life.
        * @param wind_speed The wind velocity in mi/hr.
        * @param vi         The initial velocity of the projectile (muzzle velocity).
        * @param x         The range at which you wish to determine windage, in feet.
        * @param t          The time it has taken the projectile to traverse the range x, in seconds.
        * @return The amount of windage correction, in inches, required to achieve zero on a target at the given range.
        */
        public static double windage(double wind_speed, double vi, double x, double t)
        {
            double vw = wind_speed * 17.60; // Convert to inches per second.
            return (vw * (t - x / vi));
        }

        /**
         * Resolve any wind / angle combination into headwind.
         * Headwind is positive at {@code wind_angle=0}
         * @param wind_speed The wind velocity, in mi/hr.
         * @param wind_angle The angle from which the wind is coming, in degrees.
         *                   0 degrees is from straight ahead
         *                   90 degrees is from right to left
         *                   180 degrees is from directly behind
         *                   270 or -90 degrees is from left to right.
         * @return the headwind velocity component, in mi/hr.
         */
        public static double headwind(double wind_speed, double wind_angle)
        {
            double w_angle = Angle.deg_to_rad(wind_angle);
            return (Math.Cos(w_angle) * wind_speed);
        }

        /**
         * Resolve any wind / angle combination into crosswind.
         * Positive is from Shooter's Right to Left (Wind from 90 degree)
         * @param wind_speed The wind velocity, in mi/hr.
         * @param wind_angle The angle from which the wind is coming, in degrees.
         *                   0 degrees is from straight ahead
         *                   90 degrees is from right to left
         *                   180 degrees is from directly behind
         *                   270 or -90 degrees is from left to right.
         * @return the crosswind velocity component, in mi/hr.
         */
        public static double crosswind(double wind_speed, double wind_angle)
        {
            double w_angle = Angle.deg_to_rad(wind_angle);
            return (Math.Sin(w_angle) * wind_speed);
        }
    }
}

