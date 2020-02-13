namespace skaktego
{

    public sealed class UI
    {
        // Screen size
        private const int SCREEN_WIDTH = 640;
        private const int SCREEN_HEIGHT = 480;

        private const string RESOURCE_PATH = "resources/";

        private static UI instance = null;
        private static readonly object padlock = new object();

        UI()
        {


        }

        public static UI Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new UI();
                    }
                    return instance;
                }
            }
        }
    }


}