import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import type { PublicMenuItem } from "@/types/public";
import { MenuVideoCard } from "./MenuVideoCard";

const base: PublicMenuItem & { categoryName: string } = { id: "1", categoryId: "c", categoryName: "Burger", name: "Klassik burger", description: "Təzə burger", nutrition: "", imageUrl: null, price: 12, discountPercentage: 0, finalPrice: 12, preparationTimeMinutes: 15, isAvailable: true, isPopular: false, is3DEnabled: false, has3DModel: false, enableIngredientAnimation: false, modelPosterUrl: null, videoUrl: null, videoPosterUrl: null, videoDurationSeconds: null, isVideoEnabled: false, videoDisplayOrder: 0, ingredients: [] };
function renderCard(item = base, onOpen3D = vi.fn()) { render(<MemoryRouter><MenuVideoCard item={item} restaurantSlug="demo" activeVideoId={null} onActiveChange={vi.fn()} onOpen3D={onOpen3D} /></MemoryRouter>); return onOpen3D; }

describe("MenuVideoCard 3D integration", () => {
  it("ordinary menu items remain unchanged and have no 3D button", () => { renderCard(); expect(screen.getByText("Klassik burger")).toBeInTheDocument(); expect(screen.queryByRole("button", { name: /3D baxış/i })).not.toBeInTheDocument(); });
  it("shows and activates the 3D button only for a model-backed item", async () => { const item = { ...base, is3DEnabled: true, has3DModel: true }; const open = renderCard(item); await userEvent.click(screen.getByRole("button", { name: /3D baxış/i })); expect(open).toHaveBeenCalledWith(item); });
});
