import { Component, type ErrorInfo, type ReactNode } from "react";

export class FoodViewerErrorBoundary extends Component<{ children: ReactNode; fallback: ReactNode; onError?: () => void }, { failed: boolean }> {
  state = { failed: false };
  static getDerivedStateFromError() { return { failed: true }; }
  componentDidCatch(error: Error, info: ErrorInfo) { console.error("3D model göstərilə bilmədi", error, info.componentStack); this.props.onError?.(); }
  render() { return this.state.failed ? this.props.fallback : this.props.children; }
}
