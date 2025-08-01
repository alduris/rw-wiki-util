using Menu.Remix.MixedUI;
using RWCustom;

namespace WikiUtil.Remix
{
    internal abstract class Tab(OptionInterface owner, string name) : OpTab(owner, Translate(name))
    {
        public abstract void Initialize();
        public abstract void Update();

        public static string Translate(string text) => Custom.rainWorld.inGameTranslator.TryTranslate(text, out var translated) ? translated : text;
    }
}
