using PisaciStroj.Pamat;

namespace PSMat.Testy.Pamat
{
    public class GapBufferTest
    {
        public void Spust()
        {
            OtestujMazanie();
        }

        private void OtestujMazanie()
        {
            var b = new GapBuffer();

            b.Append("        public Dictionary<int, HashSet<int>> FollowPos { get; private set; }");

            b.Delete(0, 6);

            var text = b.Read();
        }
    }
}
