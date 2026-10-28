import { Routes, Route, BrowserRouter } from "react-router-dom";
import Login from "./pages/auth/login";
import Register from "./pages/auth/register";
import Recipes from "./pages/recipes/recipes";
import RecipeDetails from "./pages/recipes/recipedetails";
import Categories from "./pages/categories/categories";
import "./App.css";
import { Layout } from "./presentation/components/Layout";
import NavBar from "./presentation/components/NavBar";
import ResetPassword from "./pages/auth/resetpassword";

function App() {
  return (
    <BrowserRouter>
      <Layout>
        <NavBar />
        <Routes>
          <Route path="/" element={<Recipes />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/recipes" element={<Recipes />} />
          <Route path="/recipes/:id" element={<RecipeDetails />} />
          <Route path="/categories" element={<Categories />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  );
}

export default App;
