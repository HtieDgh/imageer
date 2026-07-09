namespace image_proccessing.model
{
    internal class Algorithm
    {
        private Processing? alg_;
        private static Algorithm? instance_;
        public static Algorithm i()
        {
            if (instance_ is null) instance_ = new Algorithm();
            return instance_;
        }
        protected Algorithm() { }
        public void setProccessing(Processing algoithm)
        {
            alg_ = algoithm;
        }
        public void run()
        {
            alg_?.run();
        }
        public byte[]? getPixels()
        {
            return alg_?.pixels_;
        }
    }
}
