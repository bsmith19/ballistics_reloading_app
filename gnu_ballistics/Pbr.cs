using System;
namespace gnu_ballistics
{
    public enum Pbr_status
    {
        PBR_UNASSIGNED,
        PBR_VALID,
        PBR_E_TOO_FAST_VY,
        PBR_E_OUT_OF_RANGE
    }

    public class Pbr
    {
        #region PrivateVars
        Int32 mnear_zero_yards; // nearest scope/projectile intersection
        Int32 mfar_zero_yards; // furthest scope/projectile intersection
        Int32 mmin_PBR_yards; // nearest target can be for a vitals hit when aiming at center of vitals
        Int32 mmax_PBR_yards; // furthest target can be for a vitals hit when aiming at center of vitals
        Int32 msight_in_at_100yards; // Sight-in at 100 yards, in 100ths of an inch.  Positive is above center; negative is below.
        #endregion

        #region PublicProps
        public Int32 near_zero_yards { get { return mnear_zero_yards; } }
        public Int32 far_zero_yards { get { return mfar_zero_yards; } }
        public Int32 min_PBR_yards { get { return mmin_PBR_yards; } }
        public Int32 max_PBR_yards { get { return mmax_PBR_yards; } }
        public Int32 sight_in_at_100yards { get { return msight_in_at_100yards; } }
        #endregion

        #region Constructors
        public Pbr()
        {
            mnear_zero_yards = 0;
            mfar_zero_yards = 0;
            mmin_PBR_yards = 0;
            mmax_PBR_yards = 0;
            msight_in_at_100yards = 0;
        }
        #endregion

        /**
         * Solves for the maximum Point blank range and associated details.
         * @param pbr              a pointer to the pbr's results
         * @param drag_function    G1, G2, G3, G5, G6, G7, or G8
         * @param drag_coefficient The coefficient of drag for the projectile you wish to model.
         * @param vi               The projectile initial velocity.
         * @param sight_height     The height of the sighting system above the bore centerline.
                                   Most scopes are in the 1.5"-2.0" range.
         * @param vital_size
         * @return 0 if pbr exists, -1 for any errors
         */
        public Pbr_status solve(DragFunction drag_function, double drag_coefficient, double vi,
                      double sight_height, double vital_size)
        {
            double t = 0;
            double dt = 0.5 / vi;
            double v = 0;
            double vx = 0, vx1 = 0, vy = 0, vy1 = 0;
            double dv = 0, dvx = 0, dvy = 0;
            double x = 0, y = 0;
            double ShootingAngle = 0;
            double ZAngle = 0;
            double Step = 10;

            int quit = 0;

            double zero = -1;
            double farzero = 0;

            int vertex_keep = 0;
            double y_vertex = 0;
            double x_vertex = 0;

            double min_PBR_range = 0;
            int min_PBR_keep = 0;

            double max_PBR_range = 0;
            int max_PBR_keep = 0;

            int tin100 = 0;

            double Gy = Constants.GRAVITY * Math.Cos(Angle.deg_to_rad((ShootingAngle + ZAngle)));
            double Gx = Constants.GRAVITY * Math.Sin(Angle.deg_to_rad((ShootingAngle + ZAngle)));

            Pbr_status status = Pbr_status.PBR_UNASSIGNED;

            while (quit == 0)
            {

                Gy = Constants.GRAVITY * Math.Cos(Angle.deg_to_rad((ShootingAngle + ZAngle)));
                Gx = Constants.GRAVITY * Math.Sin(Angle.deg_to_rad((ShootingAngle + ZAngle)));

                vx = vi * Math.Cos(Angle.deg_to_rad(ZAngle));
                vy = vi * Math.Sin(Angle.deg_to_rad(ZAngle));

                y = -sight_height / 12;

                x = 0; y = -sight_height / 12;

                int keep = 0;
                int keep2 = 0;
                int tinkeep = 0;
                min_PBR_keep = 0;
                max_PBR_keep = 0;
                vertex_keep = 0;

                tin100 = 0;
                tinkeep = 0;

                int n = 0;
                for (t = 0; ; t = t + dt)
                {

                    status = Pbr_status.PBR_VALID;

                    vx1 = vx;
                    vy1 = vy;
                    v = Math.Pow(Math.Pow(vx, 2) + Math.Pow(vy, 2), 0.5);
                    dt = 0.5 / v;

                    // Compute acceleration using the drag function retardation
                    dv = Drag.retard(drag_function, drag_coefficient, v);
                    dvx = -(vx / v) * dv;
                    dvy = -(vy / v) * dv;

                    // Compute velocity, including the resolved gravity vectors.
                    vx = vx + dt * dvx + dt * Gx;
                    vy = vy + dt * dvy + dt * Gy;

                    // Compute position based on average velocity.
                    x = x + dt * (vx + vx1) / 2;
                    y = y + dt * (vy + vy1) / 2;

                    if (y > 0 && keep == 0 && vy >= 0)
                    {
                        zero = x;
                        keep = 1;
                    }

                    if (y < 0 && keep2 == 0 && vy <= 0)
                    {
                        farzero = x;
                        keep2 = 1;
                    }

                    if ((12 * y) > -(vital_size / 2) && min_PBR_keep == 0)
                    {
                        min_PBR_range = x;
                        min_PBR_keep = 1;
                    }

                    if ((12 * y) < -(vital_size / 2) && min_PBR_keep == 1 && max_PBR_keep == 0)
                    {
                        max_PBR_range = x;
                        max_PBR_keep = 1;
                    }

                    if (x >= 300 && tinkeep == 0)
                    {
                        tin100 = (int)((float)100 * (float)y * (float)12);
                        tinkeep = 1;
                    }


                    if (Math.Abs(vy) > Math.Abs(3 * vx))
                    {
                        status = Pbr_status.PBR_E_TOO_FAST_VY;
                        break;
                    }
                    if (n >= Constants.BALLISTICS_COMPUTATION_MAX_YARDS + 1)
                    {
                        status = Pbr_status.PBR_E_OUT_OF_RANGE;
                        break;
                    }

                    // The PBR will be maximum at the point where the vertex is 1/2 vital zone size.
                    if (vy < 0 && vertex_keep == 0)
                    {
                        y_vertex = y;
                        x_vertex = x;
                        vertex_keep = 1;
                    }

                    if (keep == 1 && keep2 == 1 && min_PBR_keep == 1 && max_PBR_keep == 1 && vertex_keep == 1 && tinkeep == 1)
                    {
                        break;
                    }
                }

                if ((y_vertex * 12) > (vital_size / 2))
                {
                    if (Step > 0) Step = -Step / 2; // Vertex too high.  Go downwards.
                }

                else if ((y_vertex * 12) <= (vital_size / 2))
                { // Vertex too low.  Go upwards.
                    if (Step < 0) Step = -Step / 2;
                }

                ZAngle += Step;

                if (Math.Abs(Step) < (0.01 / 60)) quit = 1;
            }

            if (status != Pbr_status.PBR_VALID)
            {
                return status;
            }

            mnear_zero_yards = (int) (zero/3);
            mfar_zero_yards = (int) (farzero/3);
            mmin_PBR_yards = (int) (min_PBR_range/3);
            mmax_PBR_yards = (int) (max_PBR_range/3);
            msight_in_at_100yards = tin100;

            return Pbr_status.PBR_VALID;
        }
    }
}

