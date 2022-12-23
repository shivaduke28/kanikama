namespace Kanikama
{
    public class AmbientLightModel
    {
        static AmbientLightModel instance;
        public static AmbientLightModel Instance => instance;
        static AmbientLightModel()
        {
            instance = new AmbientLightModel();
        }
    }
}
