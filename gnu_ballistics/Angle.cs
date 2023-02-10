namespace gnu_ballistics;

// All the code in this file is included in all platforms.
public static class Angle
{
    // Specialty angular conversion functions
    // Converts degrees to minutes of angle
    public static double deg_to_moa(double deg)
    {
        return deg * 60;
    }
    // Converts degrees to radians
    public static double deg_to_rad(double deg)
    {
        return deg * Math.PI / 180;
    }
    // Converts minutes of angle to degrees
    public static double moa_to_deg(double moa)
    {
        return moa / 60;
    }
    // Converts minutes of angle to radians
    public static double moa_to_rad(double moa)
    {
        return moa / 60 * Math.PI / 180;
    }
    // Converts radians to degrees
    public static double rad_to_deg(double rad)
    {
        return rad * 180 / Math.PI;
    }
    // Converts radiants to minutes of angle
    public static double rad_to_moa(double rad)
    {
        return rad * 60 * 180 / Math.PI;
    }

    /**
    * A function to determine the bore angle needed to achieve a target zero at Range yards
    * (at standard conditions and on level ground.)
    * @param drag_function    G1, G2, G3, G5, G6, G7, or G8
    * @param drag_coefficient The coefficient of drag for the projectile, for the supplied drag function.
    * @param vi               The initial velocity of the projectile, in feet/s
    * @param sight_height     The height of the sighting system above the bore centerline, in inches.
    *                         Most scopes fall in the 1.6 to 2.0 inch range.
    * @param zero_range       The range in yards, at which you wish the projectile to intersect yIntercept.
    * @param y_intercept      The height, in inches, you wish for the projectile to be when it crosses ZeroRange yards.
    *                         This is usually 0 for a target zero, but could be any number.  For example if you wish
    *                         to sight your rifle in 1.5" high at 100 yds, then you would set yIntercept to 1.5, and ZeroRange to 100
    * @return The angle of the bore relative to the sighting system, in degrees.
    */
    public static double zero_angle(DragFunction drag_function, double drag_coefficient, double vi, double sight_height, double zero_range,
                      double y_intercept)
    {
        // Numerical Integration variables
        double t = 0;
        double dt = 1 / vi; // The solution accuracy generally doesn't suffer if its within a foot for each second of time.
        double y = -sight_height / 12;
        double x = 0;
        double da; // The change in the bore angle used to iterate in on the correct zero angle.

        // State variables for each integration loop.
        double v = 0, vx = 0, vy = 0; // velocity
        double vx1 = 0, vy1 = 0; // Last frame's velocity, used for computing average velocity.
        double dv = 0, dvx = 0, dvy = 0; // acceleration
        double Gx = 0, Gy = 0; // Gravitational acceleration

        double angle = 0; // The actual angle of the bore.

        int quit = 0; // We know it's time to quit our successive approximation loop when this is 1.

        // Start with a very coarse angular change, to quickly solve even large launch angle problems.
        da = deg_to_rad(14);


        // The general idea here is to start at 0 degrees elevation, and increase the elevation by 14 degrees
        // until we are above the correct elevation.  Then reduce the angular change by half, and begin reducing
        // the angle.  Once we are again below the correct angle, reduce the angular change by half again, and go
        // back up.  This allows for a fast successive approximation of the correct elevation, usually within less
        // than 20 iterations.
        for (angle = 0; quit == 0; angle = angle + da)
        {
            vy = vi * Math.Sin(angle);
            vx = vi * Math.Cos(angle);
            Gx = Constants.GRAVITY * Math.Sin(angle);
            Gy = Constants.GRAVITY * Math.Cos(angle);

            for (t = 0, x = 0, y = -sight_height / 12; x <= zero_range * 3; t = t + dt)
            {
                vy1 = vy;
                vx1 = vx;
                v = Math.Pow((Math.Pow(vx, 2) + Math.Pow(vy, 2)), 0.5);
                dt = 1 / v;

                dv = Drag.retard(drag_function, drag_coefficient, v);
                dvy = -dv * vy / v * dt;
                dvx = -dv * vx / v * dt;

                vx = vx + dvx;
                vy = vy + dvy;
                vy = vy + dt * Gy;
                vx = vx + dt * Gx;

                x = x + dt * (vx + vx1) / 2;
                y = y + dt * (vy + vy1) / 2;
                // Break early to save CPU time if we won't find a solution.
                if (vy < 0 && y < y_intercept)
                {
                    break;
                }
                if (vy > 3 * vx)
                {
                    break;
                }
            }

            if (y > y_intercept && da > 0)
            {
                da = -da / 2;
            }

            if (y < y_intercept && da < 0)
            {
                da = -da / 2;
            }

            if (Math.Abs(da) < moa_to_rad(0.01)) quit = 1; // If our accuracy is sufficient, we can stop approximating.
            if (angle > deg_to_rad(45)) quit = 1; // If we exceed the 45 degree launch angle, then the projectile just won't get there, so we stop trying.
        }

        return rad_to_deg(angle); // Convert to degrees for return value.
    }
}

