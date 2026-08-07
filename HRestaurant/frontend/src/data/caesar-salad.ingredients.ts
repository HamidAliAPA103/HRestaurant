export const caesarSaladIngredients = [
  { meshName: "ingredient_plate", name: "Servis boşqabı", description: "Salatın təqdim olunduğu soyuq keramika boşqab.", calories: 0, allergens: "Yoxdur", explodedPosition: [0, -0.42, 0] as const },
  { meshName: "ingredient_lettuce", name: "Romaine kahısı", description: "Xırtıldayan, təzə romaine yarpaqları.", calories: 18, allergens: "Yoxdur", explodedPosition: [-1.25, 0.7, 0.15] as const },
  { meshName: "ingredient_chicken", name: "Qril toyuq", description: "Yüngül ədviyyatlı, qril edilmiş toyuq filesi.", calories: 214, allergens: "Yoxdur", explodedPosition: [1.3, 0.82, 0.08] as const },
  { meshName: "ingredient_croutons", name: "Krutonlar", description: "Kərə yağı və otlarla qızardılmış çörək parçaları.", calories: 96, allergens: "Gluten, süd", explodedPosition: [-1.08, 1.48, -0.12] as const },
  { meshName: "ingredient_parmesan", name: "Parmezan", description: "Yetkinləşdirilmiş parmezan yonqarları.", calories: 84, allergens: "Süd", explodedPosition: [1.08, 1.5, -0.16] as const },
  { meshName: "ingredient_sauce", name: "Caesar sousu", description: "Parmezan, sarımsaq və ançous notları ilə klassik sous.", calories: 142, allergens: "Yumurta, balıq, süd", explodedPosition: [0, 1.96, 0.18] as const },
] as const;

export const caesarSaladAllergens = ["Gluten", "Süd", "Yumurta", "Balıq"] as const;
