import { Component, type ErrorInfo, type ReactNode } from "react";

interface SceneErrorBoundaryProps {
  children: ReactNode;
  fallback: ReactNode;
  resetKey: string;
  onError?: (error: Error) => void;
}

interface SceneErrorBoundaryState {
  hasError: boolean;
}

export class SceneErrorBoundary extends Component<
  SceneErrorBoundaryProps,
  SceneErrorBoundaryState
> {
  public state: SceneErrorBoundaryState = { hasError: false };

  public static getDerivedStateFromError(): SceneErrorBoundaryState {
    return { hasError: true };
  }

  public componentDidUpdate(previous: SceneErrorBoundaryProps) {
    if (previous.resetKey !== this.props.resetKey && this.state.hasError) {
      this.setState({ hasError: false });
    }
  }

  public componentDidCatch(error: Error, info: ErrorInfo) {
    this.props.onError?.(error);
    console.error("3D model could not be rendered; procedural fallback activated.", {
      error,
      componentStack: info.componentStack,
    });
  }

  public render() {
    return this.state.hasError ? this.props.fallback : this.props.children;
  }
}
