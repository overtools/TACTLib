namespace TACTView.Models {
    public record RecentItem(string Target, string? Flavor, TACTType Type) {
        public override string ToString() {
            return string.IsNullOrEmpty(Flavor) ? Target : $"({Flavor}) {Target}";
        }
    }
}
