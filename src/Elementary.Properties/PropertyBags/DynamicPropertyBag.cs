namespace Elementary.Properties.PropertyBags
{
    public class DynamicPropertyBag<T> : PropertyBagBase<T>
        where T : class
    {
        public DynamicPropertyBag()
        {
        }

        public void SetInstance(T instance)
        {
            this.Instance = instance;
        }
    }
}