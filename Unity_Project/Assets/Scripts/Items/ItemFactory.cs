namespace TimefulDungeon.Items {
    public static class ItemFactory {
        public static Item GetItemFromTemplate(ItemTemplate template) {
            return template switch {
                MeleeTemplate meleeTemplate => new Melee(meleeTemplate),
                RangedTemplate rangedTemplate => new Ranged(rangedTemplate),
                ShieldTemplate shieldTemplate => new Shield(shieldTemplate),
                _ => new Item(template)
            };
        }
    }
}