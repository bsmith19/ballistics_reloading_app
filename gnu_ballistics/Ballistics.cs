using System;
namespace gnu_ballistics
{
    public struct Target
    {
        public Double range_yards;
        public Double path_inches;
        public Double moa_correction;
        public Double seconds;
        public Double windage_inches;
        public Double windage_moa;
        public Double v_fps;
        public Double vx_fps;
        public Double vy_fps;
    }

    public class Ballistics
    {
        #region PrivateVars
        Dictionary<Int32, Target> mtarget_list;
        Int32 mmax_yardage;
        #endregion

        #region PublicProperties
        public Dictionary<Int32, Target> TargetList { get { return mtarget_list; } }
        #endregion

        #region Constructors
        public Ballistics()
        {
            mtarget_list = new Dictionary<int, Target>();
            mmax_yardage = 0;
        }
        #endregion

        public Int32 solve(DragFunction drag_function, double drag_coefficient, double vi,
                             double sight_height, double shooting_angle, double zero_angle, double wind_speed, double wind_angle)
        {
            double t = 0;
            double dt = 0;
            double v = 0;
            double vx = 0, vx1 = 0, vy = 0, vy1 = 0;
            double dv = 0, dvx = 0, dvy = 0;
            double x = 0, y = 0;

            double hwind = Windage.headwind(wind_speed, wind_angle);
            double cwind = Windage.crosswind(wind_speed, wind_angle);

            double gy = Constants.GRAVITY * Math.Cos(Angle.deg_to_rad((shooting_angle + zero_angle)));
            double gx = Constants.GRAVITY * Math.Sin(Angle.deg_to_rad((shooting_angle + zero_angle)));

            vx = vi * Math.Cos(Angle.deg_to_rad(zero_angle));
            vy = vi * Math.Sin(Angle.deg_to_rad(zero_angle));

            y = -sight_height / 12; // y is in feet

            int n = 0;
            for (t = 0; ; t = t + dt)
            {
                vx1 = vx;
                vy1 = vy;
                v = Math.Pow(Math.Pow(vx, 2) + Math.Pow(vy, 2), 0.5);
                dt = 0.5 / v;

                // Compute acceleration using the drag function retardation  
                dv = Drag.retard(drag_function, drag_coefficient, v + hwind);
                dvx = -(vx / v) * dv;
                dvy = -(vy / v) * dv;

                // Compute velocity, including the resolved gravity vectors.  
                vx = vx + dt * dvx + dt * gx;
                vy = vy + dt * dvy + dt * gy;

                if (x / 3 >= n)
                {
                    Target s = new Target();
                    s.range_yards = x / 3;
                    s.path_inches = y * 12;
                    s.moa_correction = -1 * Angle.rad_to_moa(Math.Atan(y / x));
                    s.seconds = t + dt;
                    s.windage_inches = Windage.windage(cwind, vi, x, t + dt);
                    s.windage_moa = Angle.rad_to_moa(Math.Atan((s.windage_inches / 12) / x));
                    s.v_fps = v;
                    s.vx_fps = vx;
                    s.vy_fps = vy;
                    mtarget_list.Add(n,s);
                    n++;
                }

                // Compute position based on average velocity.
                x = x + dt * (vx + vx1) / 2;
                y = y + dt * (vy + vy1) / 2;

                if (Math.Abs(vy) > Math.Abs(3 * vx) || n >= Constants.BALLISTICS_COMPUTATION_MAX_YARDS) break;
            }

            mmax_yardage = n;
            return n;

        }
    }
}

