import { useContext, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useLogin } from "../../presentation/hooks/useLogin";
import { AuthContext } from "../../presentation/context/AuthContext";
import { LoadingButton } from "../../presentation/components/LoadingButton";

const getLoginErrorMessage = (err: unknown): string => {
  if (axios.isAxiosError(err)) {
    if (!err.response) return "Can't reach the server. Check your connection and try again.";
    if (err.response.status === 401) return "Invalid username or password.";
    return "Something went wrong on our side. Please try again.";
  }
  return "Login failed. Please try again.";
};

const Login = () => {
  const navigate = useNavigate();
  const { setLoginState } = useContext(AuthContext);
  const { login } = useLogin();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setIsLoading(true);

    try {
      const token = await login({ username, password });
      setLoginState(token.token);
      navigate("/recipes");
    } catch (err) {
      setError(getLoginErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container">
      <h2 style={{ textAlign: "center", marginBottom: "20px" }}>Login</h2>

      {error && (
        <p style={{ color: "red", textAlign: "center", marginBottom: "15px" }}>
          {error}
        </p>
      )}

      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: "20px" }}>
          <label>Username</label>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            disabled={isLoading}
            className="form-control"
          />
        </div>

        <div style={{ marginBottom: "20px" }}>
          <label>Password</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            disabled={isLoading}
            className="form-control"
          />
        </div>

        <LoadingButton
          type="submit"
          className="btn btn-accent"
          isLoading={isLoading}
          loadingText="Logging in..."
        >
          Login
        </LoadingButton>
      </form>

      <div style={{ marginTop: "20px", textAlign: "center" }}>
        <a href="/register" style={{ display: "block", marginBottom: "10px" }}>
          Create an account
        </a>
        <a href="/reset-password">Forgot your password</a>
      </div>
    </div>
  );
};

export default Login;
