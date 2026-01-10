public static class ElementCombiner
{
    public static Element Combine(Element a, Element b)
    {
        if (a == b) return a;

        return (a, b) switch
        {
            (Element.Fire, Element.Water) or (Element.Water, Element.Fire) => Element.Dark,
            (Element.Fire, Element.Air) or (Element.Air, Element.Fire) => Element.Storm,
            (Element.Fire, Element.Nature) or (Element.Nature, Element.Fire) => Element.Magma,

            (Element.Water, Element.Air) or (Element.Air, Element.Water) => Element.Ice,
            (Element.Water, Element.Nature) or (Element.Nature, Element.Water) => Element.Poison,

            (Element.Nature, Element.Air) or (Element.Air, Element.Nature) => Element.Earth,

            _ => Element.None
        };
    }
}
