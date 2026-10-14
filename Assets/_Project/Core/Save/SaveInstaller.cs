using System.IO;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Save
{
    // saveSystem flag: files under <persistentDataPath>/save
    public sealed class SaveInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IFileSystem, SystemFileSystem>(Lifetime.Singleton);
            builder.Register<ISaveStore>(resolver => new FileSaveStore(resolver.Resolve<IFileSystem>(),
                Path.Combine(Application.persistentDataPath, "save"), resolver.Resolve<ILog>()), Lifetime.Singleton);
        }
    }
}
