namespace HietakissaUtils.HKTransformTool
{
    abstract class HKTransformOperation
    {
        public virtual bool ShouldUpdateCommonOrigin { get; set; } = false;

        public readonly KeyCombination Combination;
        protected HKTransformTool tool;

        public HKTransformOperation(KeyCombination combination)
        {
            Combination = combination;
        }

        public void Initialize(HKTransformTool tool) => this.tool = tool;

        public abstract void OnStart();
        public abstract void OnUpdate();

        public abstract void OnApply();
        public abstract void OnCancel();
    }
}
