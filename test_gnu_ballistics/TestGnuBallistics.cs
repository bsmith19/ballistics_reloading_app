namespace test_gnu_ballistics;

[TestClass]
public class TestGnuBallistics
{
    [TestMethod]
    public void SimpleBallisticsTest()
    {
        Ballistics solution = new Ballistics();
        double bc = 0.5;
        double fps = 1200;
        double seightHeight = 1.6;
        double angle = 0;
        double zero = 100;
        double windSpeed = 0;
        double windAngle = 0;
        double zeroAngle = Angle.zero_angle(DragFunction.G1, bc, fps, 1.6, zero, 0);
        int nsoln = solution.solve(DragFunction.G1, bc, fps, seightHeight, angle, zeroAngle, windSpeed, windAngle);
        Assert.AreEqual(5090, nsoln);
        Assert.AreEqual<double>(-1.60, solution.TargetList[0].path_inches);
        Assert.AreEqual<double>(0.021086942030323762, solution.TargetList[100].path_inches);
        Assert.AreEqual<double>(-25.871778035601729, solution.TargetList[200].path_inches);
        Assert.AreEqual<double>(-82.458422497938699, solution.TargetList[300].path_inches);
        Assert.AreEqual<double>(-172.74938891261581, solution.TargetList[400].path_inches);
        Assert.AreEqual<double>(-299.58523278666632, solution.TargetList[500].path_inches);
        Assert.AreEqual<double>(-465.99531684228566, solution.TargetList[600].path_inches);
        Assert.AreEqual<double>(-674.45631229315825, solution.TargetList[700].path_inches);
        Assert.AreEqual<double>(-927.58052626524727, solution.TargetList[800].path_inches);
        Assert.AreEqual<double>(-1229.0334190298465, solution.TargetList[900].path_inches);
        Assert.AreEqual<double>(-1580.0152706594765, solution.TargetList[1000].path_inches);
    }

    [TestMethod]
    public void SimplePbrTest()
    {
        Pbr test = new Pbr();
        Assert.AreEqual(Pbr_status.PBR_VALID, test.solve(DragFunction.G1, 0.48, 2800, 1.5, 4));
        Assert.AreEqual(29, test.near_zero_yards);
        Assert.AreEqual(203, test.far_zero_yards);
        Assert.AreEqual(0, test.min_PBR_yards);
        Assert.AreEqual(238, test.max_PBR_yards);
        Assert.AreEqual(1.89, test.sight_in_at_100yards / 100.0);
    }
}
