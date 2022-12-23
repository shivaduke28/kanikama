using System;
using System.Collections.Generic;

namespace Kanikama.Baking
{
    public interface IKanikamaSceneManager
    {
        void Initialize();
        void TurnOff();
        List<ObjectReference<LightSource>> LightSources { get; }
        List<ObjectReference<KanikamaLightSourceGroup>> LightSourceGroups { get; }
        void SetDirectionalMode(bool isDirectional);
        void Rollback();
        void RollbackNonKanikama();
        void RollbackKanikama();
        void RollbackDirectionalMode();
    }
}
